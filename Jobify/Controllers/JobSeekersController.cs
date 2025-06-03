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
    public class JobSeekersController : Controller
    {
        private JobifyEntities db = new JobifyEntities();

        // GET: JobSeekers
        public ActionResult Index()
        {
            var jobSeekers = db.JobSeekers.Include(j => j.User);
            return View(jobSeekers.ToList());
        }

        // GET: JobSeekers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobSeeker jobSeeker = db.JobSeekers.Find(id);
            if (jobSeeker == null)
            {
                return HttpNotFound();
            }
            return View(jobSeeker);
        }

        // GET: JobSeekers/Create
        public ActionResult Create()
        {
            ViewBag.JobSeekerId = new SelectList(db.Users, "UserId", "FullName");
            return View();
        }

        // POST: JobSeekers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "JobSeekerId,Gender,DateOfBirth")] JobSeeker jobSeeker)
        {
            if (ModelState.IsValid)
            {
                db.JobSeekers.Add(jobSeeker);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.JobSeekerId = new SelectList(db.Users, "UserId", "FullName", jobSeeker.JobSeekerId);
            return View(jobSeeker);
        }

        // GET: JobSeekers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobSeeker jobSeeker = db.JobSeekers.Find(id);
            if (jobSeeker == null)
            {
                return HttpNotFound();
            }
            ViewBag.JobSeekerId = new SelectList(db.Users, "UserId", "FullName", jobSeeker.JobSeekerId);
            return View(jobSeeker);
        }

        // POST: JobSeekers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "JobSeekerId,Gender,DateOfBirth")] JobSeeker jobSeeker)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jobSeeker).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.JobSeekerId = new SelectList(db.Users, "UserId", "FullName", jobSeeker.JobSeekerId);
            return View(jobSeeker);
        }

        // GET: JobSeekers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobSeeker jobSeeker = db.JobSeekers.Find(id);
            if (jobSeeker == null)
            {
                return HttpNotFound();
            }
            return View(jobSeeker);
        }


        // GET: JobSeekers/Dashboard
        // GET: JobSeekers/Dashboard
        public ActionResult Dashboard()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            JobSeeker jobSeeker = db.JobSeekers.Include(j => j.User).FirstOrDefault(js => js.JobSeekerId == userId);

            if (jobSeeker == null)
            {
                return RedirectToAction("CreateProfile", "JobSeekers");
            }

            var applications = db.JobApplications
                .Include(a => a.JobPost.Category)
                .Include(a => a.JobPost.Location)
                .Include(a => a.JobPost.Employer)
                .Where(a => a.JobSeekerId == jobSeeker.JobSeekerId)
                .ToList();

            var dashboardViewModel = new JobSeekerDashboardViewModel
            {
                JobSeekerDetails = jobSeeker,
                Applications = applications.OrderByDescending(a => a.AppliedAt).Take(5).ToList(),
                TotalApplications = applications.Count,
                LatestApplicationDate = applications
                    .OrderByDescending(a => a.AppliedAt)
                    .FirstOrDefault()?.AppliedAt?.ToString("yyyy-MM-dd")
            };

            // --- Populate Data for Applications Trend Chart (Last 7 Days) ---
            dashboardViewModel.ApplicationsByWeekLabels = new List<string>();
            dashboardViewModel.ApplicationsByWeekData = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                DateTime date = DateTime.Today.AddDays(-i);
                dashboardViewModel.ApplicationsByWeekLabels.Add(date.DayOfWeek.ToString().Substring(0, 3));
                int count = applications.Count(a => a.AppliedAt.HasValue && a.AppliedAt.Value.Date == date);
                dashboardViewModel.ApplicationsByWeekData.Add(count);
            }

            // --- Populate Data for Applications Trend Chart (Last 30 Days) ---
            dashboardViewModel.ApplicationsByMonthLabels = new List<string>();
            dashboardViewModel.ApplicationsByMonthData = new List<int>();
            for (int i = 29; i >= 0; i--)
            {
                DateTime date = DateTime.Today.AddDays(-i);
                dashboardViewModel.ApplicationsByMonthLabels.Add(date.ToString("MMM dd"));
                int count = applications.Count(a => a.AppliedAt.HasValue && a.AppliedAt.Value.Date == date);
                dashboardViewModel.ApplicationsByMonthData.Add(count);
            }

            // --- Populate Data for Application Status Chart (Removed Status dependency) ---
            // Since 'Status' column is not available, this chart will now just show a single segment
            // representing the total number of applications. You might consider removing this chart
            // from the view entirely if it doesn't provide meaningful insights without status tracking.
            dashboardViewModel.ApplicationStatusLabels = new List<string> { "Total Applications Submitted" };
            dashboardViewModel.ApplicationStatusData = new List<int> { applications.Count };

            // --- Populate Total Available Job Posts for Overall Summary Chart (Removed IsActive dependency) ---
            // If you don't have 'IsActive' and don't want to use 'Deadline', this will simply count all job posts.
            // Consider if this is the desired behavior for "Total Available Job Posts."
            dashboardViewModel.TotalAvailableJobPosts = db.JobPosts.Count();

            return View(dashboardViewModel);
        }

        // POST: JobSeekers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JobSeeker jobSeeker = db.JobSeekers.Find(id);
            db.JobSeekers.Remove(jobSeeker);
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
