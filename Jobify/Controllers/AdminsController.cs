using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Jobify.Models;

namespace Jobify.Controllers
{
    public class AdminsController : Controller
    {
        private JobifyEntities db = new JobifyEntities();

        // GET: Admins
        public ActionResult Index()
        {
            var admins = db.Admins.Include(a => a.User);
            return View(admins.ToList());
        }

        // GET: Admins/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // GET: Admins/Create
        public ActionResult Create()
        {
            ViewBag.AdminId = new SelectList(db.Users, "UserId", "FullName");
            return View();
        }

        // POST: Admins/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AdminId,Department")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                db.Admins.Add(admin);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AdminId = new SelectList(db.Users, "UserId", "FullName", admin.AdminId);
            return View(admin);
        }
        //Dashboard 
        public ActionResult Dashboard()
        {
            var totalUsers = db.Users.Count();
            var totalJobPosts = db.JobPosts.Count();
            var totalApplications = db.JobApplications.Count();

            var adminCount = db.Users.Count(u => u.Role == "Admin");
            var employerCount = db.Users.Count(u => u.Role == "Employer");
            var jobSeekerCount = db.Users.Count(u => u.Role == "JobSeeker");

            // --- Dynamic Job Posts Data ---
            var currentYear = DateTime.Now.Year;
            var monthlyJobPostsData = db.JobPosts
                .Where(p => p.CreatedAt.HasValue && p.CreatedAt.Value.Year == currentYear) // Filter by current year
                .GroupBy(p => p.CreatedAt.Value.Month)
                .OrderBy(g => g.Key) // Order by month number
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToList();

            // Initialize an array for all 12 months with 0 counts
            int[] monthlyData = new int[12];
            string[] monthNames = new string[12];

            // Populate monthlyData and monthNames
            for (int i = 0; i < 12; i++)
            {
                // Get the name of the month (e.g., "January", "February")
                monthNames[i] = new DateTime(currentYear, i + 1, 1).ToString("MMM"); // "MMM" for Jan, Feb, etc.
                var monthEntry = monthlyJobPostsData.FirstOrDefault(m => m.Month == (i + 1));
                if (monthEntry != null)
                {
                    monthlyData[i] = monthEntry.Count;
                }
            }
            // --- End Dynamic Job Posts Data ---

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalJobPosts = totalJobPosts;
            ViewBag.TotalApplications = totalApplications;

            ViewBag.AdminCount = adminCount;
            ViewBag.EmployerCount = employerCount;
            ViewBag.JobSeekerCount = jobSeekerCount;

            ViewBag.MonthlyJobPosts = monthlyData;
            ViewBag.MonthlyJobPostLabels = monthNames; // Pass the month names to the view

            return View();
        }



        // GET: Admins/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            ViewBag.AdminId = new SelectList(db.Users, "UserId", "FullName", admin.AdminId);
            return View(admin);
        }

        // POST: Admins/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AdminId,Department")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                db.Entry(admin).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AdminId = new SelectList(db.Users, "UserId", "FullName", admin.AdminId);
            return View(admin);
        }

        //create 
        // GET: Admins/CreateUser

        public ActionResult CreateUser()
        {
            ViewBag.Roles = new SelectList(new[] { "Admin", "Employer", "JobSeeker" });
            return View("~/Views/Users/CreateUsersView.cshtml");
        }


        // POST: Admins/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(Jobify.ViewModels.CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new JobifyEntities())
                {
                    var existing = db.Users.FirstOrDefault(u => u.Email == model.Email);
                    if (existing != null)
                    {
                        TempData["Error"] = "Email already exists.";
                        ViewBag.Roles = new SelectList(new[] { "Admin", "Employer", "JobSeeker" });
                        return View(model);
                    }

                    var passwordHash = Crypto.HashPassword(model.Password);
                    var user = new User
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        PasswordHash = model.Password,
                        Role = model.Role,
                        CreatedAt = DateTime.Now
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    // Link to role-specific table
                    if (model.Role == "Employer")
                        db.Employers.Add(new Employer { EmployerId = user.UserId, CompanyName = "Test Company" });
                    else if (model.Role == "JobSeeker")
                        db.JobSeekers.Add(new JobSeeker { JobSeekerId = user.UserId });
                    else if (model.Role == "Admin")
                        db.Admins.Add(new Admin { AdminId = user.UserId });

                    db.SaveChanges();

                    TempData["Success"] = "User created successfully!";
                    return RedirectToAction("Index"); // or "Dashboard"
                }
            }

            ViewBag.Roles = new SelectList(new[] { "Admin", "Employer", "JobSeeker" });
            return View(model);
        }


        // GET: Admins/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Admin admin = db.Admins.Find(id);
            db.Admins.Remove(admin);
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
