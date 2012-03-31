using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Lawspot.Backend;
using Lawspot.Views.Account;

namespace Lawspot.Controllers
{
    public class AccountController : BaseController
    {
        [HttpGet]
        public ActionResult Login()
        {
            var model = new LoginViewModel();
            model.RememberMe = true;
            return View(model);
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            // Check the model is valid.
            if (ModelState.IsValid == false)
                return View(model);

            // Get the user with the given email address (if any).
            var user = this.DataContext.Users.Where(u => u.EmailAddress == model.EmailAddress).FirstOrDefault();
            if (user == null || BCrypt.Net.BCrypt.Verify(model.Password, user.Password) == false)
            {
                ModelState.AddModelError("Password", "The email or password you entered is incorrect.");
                return View(model);
            }

            // Create login cookie.
            Login(user, rememberMe: model.RememberMe);

            // Redirect to home page.
            return RedirectToAction("Index", "Home", new { alert = "loggedin" });
        }

        [HttpGet]
        public ActionResult Register()
        {
            var model = new RegisterViewModel();
            model.RegionId = 2;
            PopulateRegisterViewModel(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            // Check the password matches the confirmation password.
            if (model.Password != model.ConfirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Your two passwords do not match.");
            
            // Check the user agreed to the terms and conditions.
            if (model.Agreement == false)
                ModelState.AddModelError("Agreement", "You must agree to the terms and conditions.");

            // Check an account with the email doesn't already exist.
            if (this.DataContext.Users.Any(u => u.EmailAddress == model.EmailAddress))
                ModelState.AddModelError("EmailAddress", "That email address is already registered.");

            // Check the model is valid.
            if (ModelState.IsValid == false)
            {
                PopulateRegisterViewModel(model);
                return View(model);
            }

            // Register a new user.
            var user = new User();
            user.EmailAddress = model.EmailAddress;
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password, workFactor: 12);
            user.RegionId = model.RegionId;
            this.DataContext.Users.InsertOnSubmit(user);
            this.DataContext.SubmitChanges();

            // Log in as that user.
            Login(user, rememberMe: true);

            // Redirect to home page.
            return RedirectToAction("Index", "Home", new { alert = "registered" });
        }

        /// <summary>
        /// Populate model information that isn't included in the POST data.
        /// </summary>
        /// <param name="model"></param>
        private void PopulateRegisterViewModel(RegisterViewModel model)
        {
            model.Regions = this.DataContext.Regions.Select(r => new SelectListItem()
            {
                Value = r.RegionId.ToString(),
                Text = r.Name,
                Selected = model.RegionId == r.RegionId,
            });
        }

        [HttpGet]
        public ActionResult LawyerRegister()
        {
            var model = new LawyerRegisterViewModel();
            model.RegionId = 2;
            PopulateLawyerRegisterViewModel(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult LawyerRegister(LawyerRegisterViewModel model)
        {
            // Check the password matches the confirmation password.
            if (model.Password != model.ConfirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Your two passwords do not match.");

            // Check the user agreed to the terms of engagement.
            if (model.Agreement == false)
                ModelState.AddModelError("Agreement", "You must agree to the terms of engagement.");

            // Check an account with the email doesn't already exist.
            if (this.DataContext.Users.Any(u => u.EmailAddress == model.EmailAddress))
                ModelState.AddModelError("EmailAddress", "That email address is already registered.");

            // Check the model is valid.
            if (ModelState.IsValid == false)
            {
                PopulateLawyerRegisterViewModel(model);
                return View(model);
            }

            // Register a new user.
            var user = new User();
            user.EmailAddress = model.EmailAddress;
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password, workFactor: 12);
            user.RegionId = model.RegionId;
            this.DataContext.Users.InsertOnSubmit(user);

            // Register a new lawyer.
            var lawyer = new Lawyer();
            lawyer.User = user;
            lawyer.FirstName = model.FirstName;
            lawyer.LastName = model.LastName;
            lawyer.PractisingCertNumber = long.Parse(model.PractisingCertNumber);
            lawyer.YearOfAdmission = model.YearAdmitted;
            lawyer.SpecialisationCategoryId = model.SpecialisationCategoryId == 0 ? (int?)null : model.SpecialisationCategoryId;
            lawyer.FirmName = model.FirmName;
            this.DataContext.Lawyers.InsertOnSubmit(lawyer);

            // Save.
            this.DataContext.SubmitChanges();

            // Log in as the new user.
            Login(user, rememberMe: true);

            // Redirect to home page.
            return RedirectToAction("Index", "Home", new { alert = "registered-as-lawyer" });
        }

        /// <summary>
        /// Populate model information that isn't included in the POST data.
        /// </summary>
        /// <param name="model"></param>
        private void PopulateLawyerRegisterViewModel(LawyerRegisterViewModel model)
        {
            PopulateRegisterViewModel(model);
            model.AdmissionYears = Enumerable.Range(1960, DateTime.Now.Year - 1960 + 1).Reverse().Select(year => new SelectListItem()
            {
                Text = year.ToString(),
                Value = year.ToString(),
                Selected = model.YearAdmitted == year
            });
            model.Categories = this.DataContext.Categories.Select(c => new SelectListItem()
            {
                Text = c.Name,
                Value = c.CategoryId.ToString(),
                Selected = model.SpecialisationCategoryId == c.CategoryId
            });
        }

        /// <summary>
        /// Creates an authentication cookie on the user's computer.
        /// </summary>
        /// <param name="user"> The user details. </param>
        /// <param name="rememberMe"> <c>true</c> to make the cookie persistant. </param>
        private void Login(User user, bool rememberMe)
        {
            var ticket = Lawspot.Shared.CustomPrincipal.FromUser(user).ToTicket(rememberMe);
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            cookie.Path = FormsAuthentication.FormsCookiePath;
            if (rememberMe)
                cookie.Expires = DateTime.Now.AddYears(1);  // Remember Me = good for one year.
            this.Response.Cookies.Add(cookie);
        }

        [HttpGet]
        public ActionResult Logout()
        {
            // Remove the authentication cookie.
            FormsAuthentication.SignOut();

            // Redirect to home page.
            return RedirectToAction("Index", "Home", new { alert = "loggedout" });
        }
    }
}
