using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Jobify.Models;
using Jobify.ViewModels;
using System.Web.Helpers;
using System.Web.Security;

namespace Jobify.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Jobify.ViewModels.RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new JobifyEntities())
                {
                    var existing = db.Users.FirstOrDefault(u => u.Email == model.Email);
                    if (existing != null)
                    {
                        // Changed to ModelState.AddModelError for front-end display
                        ModelState.AddModelError("Email", "This email address is already registered.");
                        return View(model);
                    }

                    var passwordHash = Crypto.HashPassword(model.Password);
                    var user = new User
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        PasswordHash = passwordHash,
                        Role = model.Role,
                        CreatedAt = DateTime.Now
                    };
                    db.Users.Add(user);
                    db.SaveChanges();

                    // Add to role-specific table
                    if (model.Role == "Employer")
                        db.Employers.Add(new Employer
                        {
                            EmployerId = user.UserId,
                            CompanyName = "Test Company", // Consider adding a field for CompanyName in RegisterViewModel
                        });
                    else if (model.Role == "JobSeeker")
                        db.JobSeekers.Add(new JobSeeker { JobSeekerId = user.UserId });
                    else if (model.Role == "Admin")
                        db.Admins.Add(new Admin { AdminId = user.UserId });

                    db.SaveChanges();

                    TempData["Success"] = "Registration successful! Please login.";
                    return RedirectToAction("Login");
                }
            }

            // If ModelState is not valid, redisplay the form with validation errors
            return View(model);
        }

        // GET: /Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Jobify.ViewModels.LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new JobifyEntities())
                {
                    var user = db.Users.FirstOrDefault(u => u.Email == model.Email);

                    // Check if user exists AND password matches
                    if (user != null && Crypto.VerifyHashedPassword(user.PasswordHash, model.Password))
                    {
                        FormsAuthentication.SetAuthCookie(user.Email, model.RememberMe);
                        Session["UserId"] = user.UserId;
                        Session["FullName"] = user.FullName;
                        Session["Role"] = user.Role;
                        Session["CompanyWebsite"] = null; // Ensure this is set appropriately or removed if not needed here

                        // Role-based redirection
                        switch (user.Role)
                        {
                            case "Admin":
                                return RedirectToAction("Dashboard", "Admins");
                            case "Employer":
                                return RedirectToAction("Dashboard", "Employers");
                            case "JobSeeker":
                                return RedirectToAction("Dashboard", "JobSeekers");
                            default:
                                return RedirectToAction("Index", "Home");
                        }
                    }

                    // If user is null OR password does not match
                    // Add a model error that will be displayed by ValidationSummary or ValidationMessageFor
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }

            // If ModelState is not valid or login failed, return to the view with the model
            return View(model);
        }

        // GET: /Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}