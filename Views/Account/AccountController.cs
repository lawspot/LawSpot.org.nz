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
            if (this.Request.UrlReferrer != null)
                model.RedirectUrl = this.Request.UrlReferrer.ToString();
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

            // Redirect to the original referrer, or to the home page.
            if (string.IsNullOrEmpty(model.RedirectUrl) == false)
                return Redirect(SetUriParameter(new Uri(model.RedirectUrl), "alert", "loggedin").ToString());
            return RedirectToAction("Home", "Browse", new { alert = "loggedin" });
        }

        [HttpGet]
        public ActionResult Register()
        {
            var model = new RegisterViewModel();
            model.RegionId = 2;
            if (this.Request.UrlReferrer != null)
                model.RedirectUrl = this.Request.UrlReferrer.ToString();
            PopulateRegisterViewModel(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            // Check the model is valid.
            if (ModelState.IsValid == false)
            {
                PopulateRegisterViewModel(model);
                return View(model);
            }

            // Check an account with the email doesn't already exist.
            string alert;
            var user = this.DataContext.Users.FirstOrDefault(u => u.EmailAddress == model.EmailAddress);
            if (user != null)
            {
                // Check the password is okay.
                if (BCrypt.Net.BCrypt.Verify(model.Password, user.Password) == false)
                {
                    ModelState.AddModelError("EmailAddress", "That email address is already registered.");
                    PopulateRegisterViewModel(model);
                    return View(model);
                }

                // The user tried to register, but they got the email address and password
                // right, so we'll just log them in.
                user.RegionId = model.RegionId;

                // Alert the user that they were logged in, rather than registering.
                alert = "loggedin";
            }
            else
            {
                // Register a new user.
                user = new User();
                user.EmailAddress = model.EmailAddress;
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password, workFactor: 12);
                user.RegionId = model.RegionId;
                this.DataContext.Users.InsertOnSubmit(user);

                // Alert the user that they have registered successfully.
                alert = "registered";
            }
            this.DataContext.SubmitChanges();

            // Log in as that user.
            Login(user, rememberMe: true);

            // Send the user an email to thank them for registering.
            var message = new Lawspot.Email.RegisterTemplate();
            message.To.Add(user.EmailAddress);
            message.Send();

            // Redirect to the original referrer, or to the home page.
            if (string.IsNullOrEmpty(model.RedirectUrl) == false)
                return Redirect(SetUriParameter(new Uri(model.RedirectUrl), "alert", alert).ToString());
            return RedirectToAction("Home", "Browse", new { alert = alert });
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
            user.IsLawyer = true;
            this.DataContext.Users.InsertOnSubmit(user);

            // Register a new lawyer.
            var lawyer = new Lawyer();
            lawyer.User = user;
            lawyer.FirstName = model.FirstName;
            lawyer.LastName = model.LastName;
            lawyer.YearOfAdmission = model.YearAdmitted;
            lawyer.SpecialisationCategoryId = model.SpecialisationCategoryId == 0 ? (int?)null : model.SpecialisationCategoryId;
            lawyer.FirmName = model.FirmName;
            this.DataContext.Lawyers.InsertOnSubmit(lawyer);

            // Save.
            this.DataContext.SubmitChanges();

            // Log in as the new user.
            Login(user, rememberMe: true);

            // Redirect to home page.
            return RedirectToAction("Home", "Browse", new { alert = "registered-as-lawyer" });
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

        [HttpGet]
        public ActionResult Logout()
        {
            // Remove the authentication cookie.
            FormsAuthentication.SignOut();

            // Redirect to the referrer, or to the home page.
            if (this.Request.UrlReferrer != null)
                return Redirect(SetUriParameter(this.Request.UrlReferrer, "alert", "loggedout").ToString());
            return RedirectToAction("Home", "Browse", new { alert = "loggedout" });
        }
    }
}
