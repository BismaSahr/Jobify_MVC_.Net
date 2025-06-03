using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Jobify.Models;
using Jobify.ViewModels;

namespace Jobify.Controllers
{
    public class JobPostsController : Controller
    {
        private JobifyEntities db = new JobifyEntities();

        // GET: JobPosts
        public ActionResult Index()
        {
            var jobPosts = db.JobPosts.Include(j => j.Category).Include(j => j.Employer).Include(j => j.Location);
            return View(jobPosts.ToList());
        }

        // GET: JobPosts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobPost jobPost = db.JobPosts.Find(id);
            if (jobPost == null)
            {
                return HttpNotFound();
            }
            return View(jobPost);
        }

        // GET: JobPosts/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName");
            ViewBag.LocationId = new SelectList(db.Locations, "LocationId", "City");

            string role = Session["Role"] as string;
            if (role == "Employer")
            {
                // Check if company website exists in session
                if (Session["CompanyWebsite"] == null || string.IsNullOrWhiteSpace(Session["CompanyWebsite"].ToString()))
                {
                    TempData["CompanyInfoError"] = "Please complete your company details before creating a job.";
                    return RedirectToAction("EditProfile", "Users");
                }
            }
            if (role == "Admin")
            {
                ViewBag.EmployerId = new SelectList(db.Employers, "EmployerId", "CompanyName");
            }

            return View();
        }


        // POST: JobPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "JobId,Title,Description,EmployerId,CategoryId,LocationId,SalaryRange,Deadline")] JobPost jobPost)
        {
            if (ModelState.IsValid)
            {
                string role = Session["Role"] as string;

                if (role == "Employer")
                {
                    int userId = Convert.ToInt32(Session["UserId"]);
                    var employer = db.Employers.FirstOrDefault(e => e.User.UserId == userId);

                    if (employer == null)
                    {
                        ModelState.AddModelError("", "Employer not found for current user.");
                        return View(jobPost);
                    }

                    jobPost.EmployerId = employer.EmployerId;
                }
                // Else, if Admin: use the EmployerId provided from the form
                jobPost.CreatedAt = DateTime.Now;
                db.JobPosts.Add(jobPost);
                db.SaveChanges();

                if (role == "Admin")
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("MyJobs", "JobPosts");
                }
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", jobPost.CategoryId);
            ViewBag.LocationId = new SelectList(db.Locations, "LocationId", "City", jobPost.LocationId);

            if ((Session["Role"] as string) == "Admin")
            {
                ViewBag.EmployerId = new SelectList(db.Employers, "EmployerId", "CompanyName", jobPost.EmployerId);
            }
          

            return View(jobPost);
        }


        // GET: JobPosts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobPost jobPost = db.JobPosts.Find(id);
            if (jobPost == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", jobPost.CategoryId);
            ViewBag.EmployerId = new SelectList(db.Employers, "EmployerId", "CompanyName", jobPost.EmployerId);
            ViewBag.LocationId = new SelectList(db.Locations, "LocationId", "City", jobPost.LocationId);
            return View(jobPost);
        }

        // POST: JobPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "JobId,Title,Description,EmployerId,CategoryId,LocationId,SalaryRange,Deadline")] JobPost jobPost)
        {
            if (ModelState.IsValid)
            {
                var role = Session["Role"] as string;
                db.Entry(jobPost).State = EntityState.Modified;
                db.SaveChanges();
                if (role == "Admin")
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("MyJobs", "JobPosts");
                }
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", jobPost.CategoryId);
            ViewBag.EmployerId = new SelectList(db.Employers, "EmployerId", "CompanyName", jobPost.EmployerId);
            ViewBag.LocationId = new SelectList(db.Locations, "LocationId", "City", jobPost.LocationId);
            

            return View(jobPost);
        }

        //Myjobs 
        public ActionResult MyJobs()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

       
            

            // Now fetch job posts for this employer
            var myJobs = (from job in db.JobPosts
                          join category in db.Categories on job.CategoryId equals category.CategoryId
                          join location in db.Locations on job.LocationId equals location.LocationId
                          where job.EmployerId == userId
                          select new Jobify.ViewModels.JobPostViewModel
                          {
                              JobId = job.JobId,
                              Title = job.Title,
                              Description = job.Description,
                              CategoryName = category.CategoryName,
                              LocationName = location.City,
                              SalaryRange = job.SalaryRange,
                              Deadline = job.Deadline,
                              CreatedAt = job.CreatedAt
                          }).ToList();

            return View(myJobs);
        }

        // GET: JobPosts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobPost jobPost = db.JobPosts.Find(id);
            if (jobPost == null)
            {
                return HttpNotFound();
            }
            return View(jobPost);
        }

        // POST: JobPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JobPost jobPost = db.JobPosts.Find(id);
            db.JobPosts.Remove(jobPost);
            db.SaveChanges();
            return RedirectToAction("Index","Home");
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
