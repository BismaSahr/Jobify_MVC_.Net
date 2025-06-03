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
    public class EmployersController : Controller
    {
        private JobifyEntities db = new JobifyEntities();

        // GET: Employers
        public ActionResult Index()
        {
            var employers = db.Employers.Include(e => e.User);
            return View(employers.ToList());
        }

        // GET: Employers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employer employer = db.Employers.Find(id);
            if (employer == null)
            {
                return HttpNotFound();
            }
            return View(employer);
        }

        // GET: Employers/Create
        public ActionResult Create()
        {
            ViewBag.EmployerId = new SelectList(db.Users, "UserId", "FullName");
            return View();
        }

        // POST: Employers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EmployerId,CompanyName,Website,Address,Designation")] Employer employer)
        {
            if (ModelState.IsValid)
            {
                db.Employers.Add(employer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.EmployerId = new SelectList(db.Users, "UserId", "FullName", employer.EmployerId);
            return View(employer);
        }

        // GET: Employers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employer employer = db.Employers.Find(id);
            if (employer == null)
            {
                return HttpNotFound();
            }
            ViewBag.EmployerId = new SelectList(db.Users, "UserId", "FullName", employer.EmployerId);
            return View(employer);
        }

        // POST: Employers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EmployerId,CompanyName,Website,Address,Designation")] Employer employer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EmployerId = new SelectList(db.Users, "UserId", "FullName", employer.EmployerId);
            return View(employer);
        }

        // GET: Employers/Dashboard
        // GET: Employers/Dashboard
        public ActionResult Dashboard()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int employerId = Convert.ToInt32(Session["UserId"]);
            Employer employer = db.Employers.Find(employerId);

            if (employer == null)
            {
                return HttpNotFound();
            }

            var postedJobs = (from job in db.JobPosts
                              join category in db.Categories on job.CategoryId equals category.CategoryId
                              join location in db.Locations on job.LocationId equals location.LocationId
                              where job.EmployerId == employerId
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

            var employerJobIds = db.JobPosts
                                   .Where(jp => jp.EmployerId == employerId)
                                   .Select(jp => jp.JobId)
                                   .ToList();

            int totalApplications = db.JobApplications
                                      .Count(ja => employerJobIds.Contains(ja.JobId));

            // --- New logic for graph data ---
            var jobPostsByWeekday = new int[7]; // Array to hold counts for Mon-Sun
            string[] weekdays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            // Initialize counts to 0
            for (int i = 0; i < 7; i++)
            {
                jobPostsByWeekday[i] = 0;
            }

            foreach (var job in postedJobs)
            {
                if (job.CreatedAt.HasValue)
                {
                    // DayOfWeek is 0 for Sunday, 1 for Monday, ..., 6 for Saturday
                    // We want Monday to be index 0, so adjust
                    int dayIndex = ((int)job.CreatedAt.Value.DayOfWeek + 6) % 7;
                    jobPostsByWeekday[dayIndex]++;
                }
            }

            // Convert to List<string> for labels and List<int> for data
            List<string> graphLabels = weekdays.ToList();
            List<int> graphData = jobPostsByWeekday.ToList();
            // ------------------------------------

            var dashboardViewModel = new EmployerDashboardViewModel
            {
                EmployerDetails = employer,
                PostedJobs = postedJobs,
                TotalPostedJobs = postedJobs.Count,
                TotalApplications = totalApplications,
                JobPostGraphLabels = graphLabels,   // <--- Assign graph labels
                JobPostGraphData = graphData        // <--- Assign graph data
            };

            return View(dashboardViewModel);
        }
        // GET: Employers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employer employer = db.Employers.Find(id);
            if (employer == null)
            {
                return HttpNotFound();
            }
            return View(employer);
        }


       
        // POST: Employers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Employer employer = db.Employers.Find(id);
            db.Employers.Remove(employer);
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
