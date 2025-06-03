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
    public class JobApplicationsController : Controller
    {
        private JobifyEntities db = new JobifyEntities();

        // GET: JobApplications
        public ActionResult Index()
        {
            var jobApplications = db.JobApplications.Include(j => j.JobPost).Include(j => j.JobSeeker);
            return View(jobApplications.ToList());
        }

        // GET: JobApplications/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobApplication jobApplication = db.JobApplications.Find(id);
            if (jobApplication == null)
            {
                return HttpNotFound();
            }
            return View(jobApplication);
        }

        // GET: JobApplications/Create
        public ActionResult Create()
        {
            ViewBag.JobId = new SelectList(db.JobPosts, "JobId", "Title"); ViewBag.JobSeekerId = new SelectList(
        db.JobSeekers
            .Join(db.Users,
                  js => js.JobSeekerId,
                  u => u.UserId,
                  (js, u) => new {
                      JobSeekerId = js.JobSeekerId,
                      FullName = u.FullName
                  }),
        "JobSeekerId",
        "FullName"
    );
            return View();
        }

        // POST: JobApplications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ApplicationId,JobId,JobSeekerId,FullName,Email,Phone,Skills,CoverLetter,ResumeLink")] JobApplication jobApplication)
        {
            if (ModelState.IsValid)
            {
                jobApplication.AppliedAt = DateTime.Now;
                db.JobApplications.Add(jobApplication);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.JobId = new SelectList(db.JobPosts, "JobId", "Title", jobApplication.JobId);
            ViewBag.JobSeekerId = new SelectList(
    db.JobSeekers
        .Join(db.Users,
              js => js.JobSeekerId,
              u => u.UserId,
              (js, u) => new {
                  JobSeekerId = js.JobSeekerId,
                  FullName = u.FullName
              }),
    "JobSeekerId",
    "FullName",
    jobApplication.JobSeekerId
);
            return View(jobApplication);
        }

        // GET: JobApplications/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobApplication jobApplication = db.JobApplications.Find(id);
            if (jobApplication == null)
            {
                return HttpNotFound();
            }
            ViewBag.JobId = new SelectList(db.JobPosts, "JobId", "Title", jobApplication.JobId);
            ViewBag.JobSeekerId = new SelectList(db.JobSeekers, "JobSeekerId", "Gender", jobApplication.JobSeekerId);
            return View(jobApplication);
        }

        // POST: JobApplications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ApplicationId,JobId,JobSeekerId,FullName,Email,Phone,Skills,CoverLetter,ResumeLink")] JobApplication jobApplication)
        {
            if (ModelState.IsValid)
            {
                jobApplication.AppliedAt = DateTime.Now;
                db.Entry(jobApplication).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.JobId = new SelectList(db.JobPosts, "JobId", "Title", jobApplication.JobId);
            ViewBag.JobSeekerId = new SelectList(db.JobSeekers, "JobSeekerId", "Gender", jobApplication.JobSeekerId);
            return View(jobApplication);
        }

        // GET: JobApplications/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobApplication jobApplication = db.JobApplications.Find(id);
            if (jobApplication == null)
            {
                return HttpNotFound();
            }
            return View(jobApplication);
        }

        // POST: JobApplications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JobApplication jobApplication = db.JobApplications.Find(id);
            db.JobApplications.Remove(jobApplication);
            db.SaveChanges();
            return RedirectToAction("Index");
        }




      
        public ActionResult EmployerJobApplications()
        {
            int? loggedInUserId = Session["UserId"] as int?;

            if (!loggedInUserId.HasValue || loggedInUserId.Value == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "User ID not found in session. Please log in.");
            }

            // Find the Employer linked to this UserId (corrected lookup)
            var employer = db.Employers.FirstOrDefault(e => e.EmployerId == loggedInUserId.Value);

            if (employer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "No employer profile found for the logged-in user with this User ID.");
            }

            var applicationsForEmployerJobs = db.JobApplications
                                            .Include(ja => ja.JobPost.Category)
                                            .Include(ja => ja.JobPost.Location)
                                            .Include(ja => ja.JobSeeker.User) // Include User through JobSeeker
                                            .Where(ja => ja.JobPost.EmployerId == employer.EmployerId)
                                            .ToList();

            var groupedApplications = applicationsForEmployerJobs
                .GroupBy(ja => ja.JobId)
                .Select(group => new JobApplicationsGroupViewModel
                {
                    JobId = group.Key,
                    JobTitle = group.First().JobPost.Title,
                    CategoryName = group.First().JobPost.Category.CategoryName,
                    LocationName = group.First().JobPost.Location.City, // Using Location.City as per your JobPostsController
                    CreatedAt = group.First().JobPost.CreatedAt, // Changed to CreatedAt
                    TotalApplications = group.Count(),
                    Applications = group.Select(ja => new JobApplicationDetailViewModel
                    {
                        ApplicationId = ja.ApplicationId,
                        JobId = ja.JobId,
                        JobTitle = ja.JobPost.Title,
                        JobCategoryName = ja.JobPost.Category.CategoryName,
                        JobLocationName = ja.JobPost.Location.City, // Using Location.City
                        JobCreatedAt = ja.JobPost.CreatedAt ?? DateTime.MinValue, // Changed to JobCreatedAt
                        JobSeekerId = ja.JobSeekerId,
                        JobSeekerFullName = ja.JobSeeker.User.FullName,
                        JobSeekerEmail = ja.JobSeeker.User.Email,
                        JobSeekerPhone = ja.JobSeeker.User.Phone, // Using User.Phone as per your code
                        JobSeekerGender = ja.JobSeeker.Gender,
                        FullName = ja.FullName,
                        Email = ja.Email,
                        Phone = ja.Phone,
                        Skills = ja.Skills,
                        CoverLetter = ja.CoverLetter,
                        ResumeLink = ja.ResumeLink,
                        AppliedAt = ja.AppliedAt ?? DateTime.MinValue
                    }).OrderByDescending(a => a.AppliedAt).ToList()
                })
                .OrderByDescending(g => g.CreatedAt) // Order the groups by Job CreatedAt
                .ToList();

            var viewModel = new EmployerJobApplicationsViewModel
            {
              EmployerName = employer.CompanyName, // Using CompanyName as implied by your JobPostsController
                GroupedJobApplications = groupedApplications
            };

            return View(viewModel);
        }


        public ActionResult JobSeekerApplications()
        {
            int? loggedInUserId = Session["UserId"] as int?;

            if (!loggedInUserId.HasValue || loggedInUserId.Value == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "User ID not found in session. Please log in.");
            }

            // Find the JobSeeker linked to this UserId (corrected lookup)
            // Note: Your current code is looking up JobSeeker by JobSeekerId == loggedInUserId.Value.
            // If Session["UserId"] stores the User.UserId, then you should lookup JobSeeker by JobSeeker.UserId.
            // I'll assume Session["UserId"] holds User.UserId, so the lookup should be:
            var jobSeeker = db.JobSeekers.FirstOrDefault(js => js.JobSeekerId == loggedInUserId.Value);

            if (jobSeeker == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "No job seeker profile found for the logged-in user.");
            }

            var jobApplications = db.JobApplications
                                    .Include(ja => ja.JobPost.Category)
                                    .Include(ja => ja.JobPost.Location)
                                    .Include(ja => ja.JobSeeker.User) // Include User through JobSeeker
                                    .Where(ja => ja.JobSeekerId == jobSeeker.JobSeekerId)
                                    .OrderByDescending(ja => ja.AppliedAt)
                                    .ToList();

            var applicationsViewModel = jobApplications.Select(ja => new JobApplicationDetailViewModel
            {
                ApplicationId = ja.ApplicationId,
                JobId = ja.JobId,
                JobTitle = ja.JobPost.Title,
                JobCategoryName = ja.JobPost.Category.CategoryName,
                JobLocationName = ja.JobPost.Location.City, // Using Location.City
                JobCreatedAt = ja.JobPost.CreatedAt ?? DateTime.MinValue, // Changed to JobCreatedAt
                JobSeekerId = ja.JobSeekerId,
                JobSeekerFullName = ja.JobSeeker.User.FullName,
                JobSeekerEmail = ja.JobSeeker.User.Email,
                JobSeekerPhone = ja.JobSeeker.User.Phone, // Using User.Phone
                JobSeekerGender = ja.JobSeeker.Gender,
                FullName = ja.FullName,
                Email = ja.Email,
                Phone = ja.Phone,
                Skills = ja.Skills,
                CoverLetter = ja.CoverLetter,
                ResumeLink = ja.ResumeLink,
                AppliedAt = ja.AppliedAt ?? DateTime.MinValue
            }).ToList();

            ViewBag.JobSeekerName = jobSeeker.User.FullName;

            return View(applicationsViewModel);
        }




        // GET: JobApplications/Create
        public ActionResult CreateApplication(int? jobId)
        {
            var userId = Session["UserId"];
            var role = Session["Role"] as string;

            if (userId == null || role != "JobSeeker")
            {
                TempData["ErrorMessage"] = "Please log in as a Job Seeker to apply for jobs.";
                return RedirectToAction("Login", "Account");
            }

            if (!jobId.HasValue)
            {
                TempData["ErrorMessage"] = "No job selected for application.";
                return RedirectToAction("Index", "Home");
            }

            var jobPost = db.JobPosts
                            .Include(j => j.Category)
                            .Include(j => j.Employer)
                            .Include(j => j.Location)
                            .SingleOrDefault(j => j.JobId == jobId.Value);

            if (jobPost == null)
            {
                TempData["ErrorMessage"] = "The selected job does not exist.";
                return RedirectToAction("Index", "Home");
            }

            var jobSeekerId = Convert.ToInt32(userId);
            var jobSeeker = db.JobSeekers.Include(js => js.User).SingleOrDefault(js => js.JobSeekerId == jobSeekerId);

            if (jobSeeker == null || jobSeeker.User == null)
            {
                TempData["ErrorMessage"] = "Your job seeker profile could not be found. Please complete your profile.";
                return RedirectToAction("Index", "JobSeekers");
            }

            // Check if user has already applied for this job
            var existingApplication = db.JobApplications
                                        .Any(ja => ja.JobId == jobId.Value && ja.JobSeekerId == jobSeeker.JobSeekerId);
            if (existingApplication)
            {
                TempData["InfoMessage"] = "You have already applied for this job.";
                return RedirectToAction("Details", "JobPosts", new { id = jobId.Value });
            }


            var viewModel = new JobApplicationViewModel
            {
                JobId = jobPost.JobId,
                JobTitle = jobPost.Title,
                CompanyName = jobPost.Employer?.CompanyName,
                CategoryName = jobPost.Category?.CategoryName,
                Location = $"{jobPost.Location?.City}, {jobPost.Location?.Country}",

                JobSeekerId = jobSeeker.JobSeekerId,
                JobSeekerFullName = jobSeeker.User.FullName,
                JobSeekerEmail = jobSeeker.User.Email,
                JobSeekerPhone = jobSeeker.User.Phone,

                Skills = "", // Initialize as empty
                CoverLetter = "", // Initialize as empty
                // REMOVED: ResumeLink = jobSeeker.ResumeLink;
                ResumeLink = null // Explicitly set to null or leave it to default as string is nullable
            };

            return View(viewModel);
        }

        // POST: JobApplications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateApplication(JobApplicationViewModel viewModel)
        {
            var userId = Session["UserId"];
            var role = Session["Role"] as string;

            if (userId == null || role != "JobSeeker")
            {
                TempData["ErrorMessage"] = "Please log in as a Job Seeker to apply for jobs.";
                return RedirectToAction("Login", "Account");
            }

            // Ensure JobSeekerId is correctly set from session (JobSeekerId == UserId in your setup)
            viewModel.JobSeekerId = Convert.ToInt32(userId);
            viewModel.AppliedAt = DateTime.Now;

            // Manual validation for ResumeLink if it's meant to be optional but still a valid URL if provided
            // This is handled by [Url] attribute on the ViewModel, but good to be aware.

            if (ModelState.IsValid)
            {
                var existingApplication = db.JobApplications
                                            .Any(ja => ja.JobId == viewModel.JobId && ja.JobSeekerId == viewModel.JobSeekerId);
                if (existingApplication)
                {
                    TempData["InfoMessage"] = "You have already applied for this job.";
                    return RedirectToAction("Details", "JobPosts", new { id = viewModel.JobId });
                }

                var jobApplication = new JobApplication
                {
                    JobId = viewModel.JobId,
                    JobSeekerId = viewModel.JobSeekerId,
                    FullName = viewModel.JobSeekerFullName,
                    Email = viewModel.JobSeekerEmail,
                    Phone = viewModel.JobSeekerPhone,
                    Skills = viewModel.Skills,
                    CoverLetter = viewModel.CoverLetter,
                    ResumeLink = viewModel.ResumeLink, // This will be null if the user leaves it blank
                    AppliedAt = viewModel.AppliedAt
                };

                db.JobApplications.Add(jobApplication);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Your application has been submitted successfully!";
                return RedirectToAction("JobSeekerApplications");
            }

            // If ModelState is invalid, re-render the form with existing data and errors
            // No need to repopulate SelectLists as they are not used for this ViewModel.
            return View(viewModel);
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
