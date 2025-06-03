using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Jobify.Models;

namespace Jobify.Controllers
{
    public class UsersController : Controller
    {
        private JobifyEntities db = new JobifyEntities();

        // GET: Users
        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.Admin).Include(u => u.Employer).Include(u => u.JobSeeker);
            return View(users.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Admins, "AdminId", "Department");
            ViewBag.UserId = new SelectList(db.Employers, "EmployerId", "CompanyName");
            ViewBag.UserId = new SelectList(db.JobSeekers, "JobSeekerId", "Gender");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,FullName,Email,PasswordHash,Phone,Role,CreatedAt")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Admins, "AdminId", "Department", user.UserId);
            ViewBag.UserId = new SelectList(db.Employers, "EmployerId", "CompanyName", user.UserId);
            ViewBag.UserId = new SelectList(db.JobSeekers, "JobSeekerId", "Gender", user.UserId);
            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Admins, "AdminId", "Department", user.UserId);
            ViewBag.UserId = new SelectList(db.Employers, "EmployerId", "CompanyName", user.UserId);
            ViewBag.UserId = new SelectList(db.JobSeekers, "JobSeekerId", "Gender", user.UserId);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,FullName,Email,PasswordHash,Phone,Role,CreatedAt")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Admins, "AdminId", "Department", user.UserId);
            ViewBag.UserId = new SelectList(db.Employers, "EmployerId", "CompanyName", user.UserId);
            ViewBag.UserId = new SelectList(db.JobSeekers, "JobSeekerId", "Gender", user.UserId);
            return View(user);
        }

        //Profile 
        public ActionResult Profile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var admin = db.Admins.FirstOrDefault(a => a.AdminId == user.UserId);
            var employer = db.Employers.FirstOrDefault(e => e.EmployerId == user.UserId);
            var jobSeeker = db.JobSeekers.FirstOrDefault(js => js.JobSeekerId == user.UserId);

            var profile = new UserProfileViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                CreatedAt = user.CreatedAt,

                Department = admin?.Department,

                CompanyName = employer?.CompanyName,
                Website = employer?.Website,
                Address = employer?.Address,
                Designation = employer?.Designation,

                Gender = jobSeeker?.Gender,
                DateOfBirth = jobSeeker?.DateOfBirth
            };

            return View(profile);
        }
        // Get:EditProfile
        public ActionResult EditProfile()
        {
            int? userId = (int?)Session["UserId"];
            if (userId == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Find(userId);
            if (user == null)
                return HttpNotFound();

            var model = new UserEditProfileViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role
            };

            if (user.Role == "Admin")
            {
                var admin = db.Admins.FirstOrDefault(a => a.AdminId == user.UserId);
                if (admin != null)
                {
                    model.Department = admin.Department;
                }
            }
            else if (user.Role == "JobSeeker")
            {
                var seeker = db.JobSeekers.FirstOrDefault(js => js.JobSeekerId == user.UserId);
                if (seeker != null)
                {
                    model.Gender = seeker.Gender;
                    model.DateOfBirth = seeker.DateOfBirth;
                }
            }
            else if (user.Role == "Employer")
            {
                var employer = db.Employers.FirstOrDefault(e => e.EmployerId == user.UserId);
                if (employer != null)
                {
                    model.CompanyName = employer.CompanyName;
                    model.Website = employer.Website;
                    model.Address = employer.Address;
                    model.Designation = employer.Designation;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(UserEditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.Users.Find(model.UserId);
            if (user == null)
                return HttpNotFound();

            // Update common user fields
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;

            // Update role-specific fields
            if (user.Role == "Admin")
            {
                var admin = db.Admins.FirstOrDefault(a => a.AdminId == user.UserId);
                if (admin != null)
                {
                    admin.Department = model.Department;
                }
            }
            else if (user.Role == "JobSeeker")
            {
                var seeker = db.JobSeekers.FirstOrDefault(js => js.JobSeekerId == user.UserId);
                if (seeker != null)
                {
                    seeker.Gender = model.Gender;
                    seeker.DateOfBirth = model.DateOfBirth;
                }
            }
            else if (user.Role == "Employer")
            {
                var employer = db.Employers.FirstOrDefault(e => e.EmployerId == user.UserId);
                if (employer != null)
                {
                    employer.CompanyName = model.CompanyName;
                    employer.Website = model.Website;
                    employer.Address = model.Address;
                    employer.Designation = model.Designation;
                    Session["CompanyWebsite"] = employer.Website;
                }
            }

            db.SaveChanges();
            return RedirectToAction("Profile", new { id = model.UserId });
        }

        // GET: Users/UserFullDetails/5
        public ActionResult UserFullDetails(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            var admin = db.Admins.FirstOrDefault(a => a.AdminId == user.UserId);
            var employer = db.Employers.FirstOrDefault(e => e.EmployerId == user.UserId);
            var jobSeeker = db.JobSeekers.FirstOrDefault(js => js.JobSeekerId == user.UserId);

            var model = new UserProfileViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                CreatedAt = user.CreatedAt,

                // Admin
                Department = admin?.Department,

                // Employer
                CompanyName = employer?.CompanyName,
                Website = employer?.Website,
                Address = employer?.Address,
                Designation = employer?.Designation,

                // JobSeeker
                Gender = jobSeeker?.Gender,
                DateOfBirth = jobSeeker?.DateOfBirth
            };

            return View(model);
        }

        // GET: Users/EditFullUser/5
        public ActionResult EditFullUser(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            var model = new UserProfileViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            if (user.Role == "Admin")
            {
                var admin = db.Admins.FirstOrDefault(a => a.AdminId == user.UserId);
                if (admin != null)
                {
                    model.Department = admin.Department;
                }
            }
            else if (user.Role == "Employer")
            {
                var employer = db.Employers.FirstOrDefault(e => e.EmployerId == user.UserId);
                if (employer != null)
                {
                    model.CompanyName = employer.CompanyName;
                    model.Website = employer.Website;
                    model.Address = employer.Address;
                    model.Designation = employer.Designation;
                }
            }
            else if (user.Role == "JobSeeker")
            {
                var seeker = db.JobSeekers.FirstOrDefault(js => js.JobSeekerId == user.UserId);
                if (seeker != null)
                {
                    model.Gender = seeker.Gender;
                    model.DateOfBirth = seeker.DateOfBirth;
                }
            }

            return View(model);
        }
        // POST: Users/EditFullUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFullUser(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.Users.Find(model.UserId);
            if (user == null)
                return HttpNotFound();

            // Update user fields
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;

            if (user.Role == "Admin")
            {
                var admin = db.Admins.FirstOrDefault(a => a.AdminId == user.UserId);
                if (admin != null)
                {
                    admin.Department = model.Department;
                }
            }
            else if (user.Role == "Employer")
            {
                var employer = db.Employers.FirstOrDefault(e => e.EmployerId == user.UserId);
                if (employer != null)
                {
                    employer.CompanyName = model.CompanyName;
                    employer.Website = model.Website;
                    employer.Address = model.Address;
                    employer.Designation = model.Designation;
                }
            }
            else if (user.Role == "JobSeeker")
            {
                var seeker = db.JobSeekers.FirstOrDefault(js => js.JobSeekerId == user.UserId);
                if (seeker != null)
                {
                    seeker.Gender = model.Gender;
                    seeker.DateOfBirth = model.DateOfBirth;
                }
            }

            db.SaveChanges();
            return RedirectToAction("UserFullDetails", new { id = model.UserId });
        }



        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var jobSeeker = db.JobSeekers.FirstOrDefault(js => js.JobSeekerId == id);
            if (jobSeeker != null)
                db.JobSeekers.Remove(jobSeeker);

            var employer = db.Employers.FirstOrDefault(e => e.EmployerId == id);
            if (employer != null)
                db.Employers.Remove(employer);

            var admin = db.Admins.FirstOrDefault(a => a.AdminId == id);
            if (admin != null)
                db.Admins.Remove(admin);

            var user = db.Users.Find(id);
            db.Users.Remove(user);

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
