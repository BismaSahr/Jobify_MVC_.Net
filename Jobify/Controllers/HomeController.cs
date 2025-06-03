using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity; // Make sure this is included for .Include()
using Jobify.Models;     // Make sure this is included for your models

namespace Jobify.Controllers
{
    public class HomeController : Controller
    {
        
        private JobifyEntities db = new JobifyEntities();

   
        public ActionResult Index()
        {
            //if (Session["Role"] != null)
            //{
            //    var role = Session["Role"].ToString();

            //    switch (role)
            //    {
            //        case "Admin":
            //            return RedirectToAction("Dashboard", "Admins");
            //        case "Employer":
            //            return RedirectToAction("Index", "Employers");
            //        case "JobSeeker":
            //            return RedirectToAction("Index", "JobSeekers");
            //        default:
                     
            //            break;
            //    }
            //}

            // If no role found in session, or role didn't lead to redirection,
            // show the public home page with job listings.
            var jobPosts = db.JobPosts // 'db' is now accessible here!
                             .Include(j => j.Category)
                             .Include(j => j.Employer)
                             .Include(j => j.Location)
                             .ToList();

            return View(jobPosts);
        }

        // GET: Home/About
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        // GET: Home/Contact
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

  
 
    }
}