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
    public class SavedJobsController : Controller
    {
        private JobifyEntities db = new JobifyEntities();

        // GET: SavedJobs
        public ActionResult Index()
        {
            var userId = Session["UserId"];
            var role = Session["Role"] as string;

            // Ensure the user is logged in and is a JobSeeker
            if (userId == null || role != "JobSeeker")
            {
                TempData["ErrorMessage"] = "Please log in as a Job Seeker to view your saved jobs.";
                return RedirectToAction("Login", "Account"); // Redirect to login if not authenticated or not a JobSeeker
            }

            try
            {
                var jobSeekerId = Convert.ToInt32(userId);

                // Fetch saved jobs for the current job seeker
                // and include all related JobPost data (Category, Employer, Location)
                var savedJobPosts = db.SavedJobs
                                      .Where(s => s.JobSeekerId == jobSeekerId)
                                      .Select(s => s.JobPost) // Select the JobPost directly
                                      .Include(j => j.Category)
                                      .Include(j => j.Employer)
                                      .Include(j => j.Location)
                                      .ToList();

                // The view expects IEnumerable<Jobify.Models.JobPost>, which is what we are now passing
                return View(savedJobPosts);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving saved jobs: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading your saved jobs. Please try again.";
                return RedirectToAction("Index", "Home"); // Redirect to home page in case of error
            }
        }

 

        // GET: SavedJobs/Create
        // This GET action for Create is likely not needed if saving is done via POST from a job listing.
        public ActionResult Create()
        {
            ViewBag.JobId = new SelectList(db.JobPosts, "JobId", "Title");
            return View();
        }

        // POST: SavedJobs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SavedJob model)
        {
            var userId = Session["UserId"];
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please log in to save jobs.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var jobSeekerId = Convert.ToInt32(userId);
                var existing = db.SavedJobs.FirstOrDefault(s => s.JobId == model.JobId && s.JobSeekerId == jobSeekerId);

                if (existing != null)
                {
                    TempData["InfoMessage"] = "You have already saved this job.";
                    return RedirectToAction("Index", "Home");
                }

                var savedJob = new SavedJob
                {
                    JobId = model.JobId,
                    JobSeekerId = jobSeekerId,
                    SavedAt = DateTime.Now
                };

                db.SavedJobs.Add(savedJob);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Job saved successfully!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving job: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to save job. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: SavedJobs/Edit/5 (Optional, typically not needed for SavedJobs unless saving status itself is edited)
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SavedJob savedJob = db.SavedJobs.Find(id);
            if (savedJob == null)
            {
                return HttpNotFound();
            }
            ViewBag.JobId = new SelectList(db.JobPosts, "JobId", "Title", savedJob.JobId);
            ViewBag.JobSeekerId = new SelectList(db.JobSeekers, "JobSeekerId", "Gender", savedJob.JobSeekerId);
            return View(savedJob);
        }

        // POST: SavedJobs/Edit/5 (Optional)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SavedJobId,JobId,JobSeekerId,SavedAt")] SavedJob savedJob)
        {
            if (ModelState.IsValid)
            {
                db.Entry(savedJob).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.JobId = new SelectList(db.JobPosts, "JobId", "Title", savedJob.JobId);
            ViewBag.JobSeekerId = new SelectList(db.JobSeekers, "JobSeekerId", "Gender", savedJob.JobSeekerId);
            return View(savedJob);
        }


        // IMPORTANT CHANGE: This is the primary Delete POST action.
        // It should match the parameter name sent from the view ('jobId').
        // It also handles authentication and specific saved job deletion.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Removed ActionName("Delete") because the method name is now "Delete"
        // And changed parameter 'id' to 'jobId'
        public ActionResult Delete(int jobId)
        {
            var userId = Session["UserId"];
            var role = Session["Role"] as string;

            if (userId == null || role != "JobSeeker")
            {
                TempData["ErrorMessage"] = "Please log in as a Job Seeker to unsave jobs.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var jobSeekerId = Convert.ToInt32(userId);
                // Find the specific SavedJob entry to delete using both JobId and JobSeekerId
                SavedJob savedJob = db.SavedJobs.SingleOrDefault(s => s.JobId == jobId && s.JobSeekerId == jobSeekerId);

                if (savedJob == null)
                {
                    TempData["InfoMessage"] = "This job is not currently saved.";
                    return RedirectToAction("Index"); // Redirect back to saved jobs list
                }

                db.SavedJobs.Remove(savedJob);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Job unsaved successfully.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error unsaving job: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to unsave job. Please try again.";
            }

            return RedirectToAction("Index"); // Always redirect back to the saved jobs list
        }


        // REMOVE OR COMMENT OUT THIS METHOD! It's the one expecting 'id' and causing the error.
        // public ActionResult Delete(int? id) { ... } // GET action to show delete confirmation page
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        // public ActionResult DeleteConfirmed(int id) // This is the old POST action that caused the error
        // {
        //     SavedJob savedJob = db.SavedJobs.Find(id);
        //     db.SavedJobs.Remove(savedJob);
        //     db.SaveChanges();
        //     return RedirectToAction("Index");
        // }


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