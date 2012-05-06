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
        /// <summary>
        /// Displays the login page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Login()
        {
            // If the user is logged in already, redirect to the home page.
            if (this.User != null)
                return RedirectToAction("Home", "Browse", new { alert = this.Request.QueryString["alert"] });

            var model = new LoginViewModel();
            model.RememberMe = true;
            if (this.Request.UrlReferrer != null)
                model.RedirectUrl = this.Request.UrlReferrer.ToString();
            if (this.Request.QueryString["ReturnUrl"] != null)
                model.RedirectUrl = this.Request.QueryString["ReturnUrl"];
            return View(model);
        }

        /// <summary>
        /// Logs the user in.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            // Check the model is valid.
            if (ModelState.IsValid == false)
                return View(model);

            // Trim the text fields.
            model.EmailAddress = model.EmailAddress.Trim();

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
                return Redirect(SetUriParameter(new Uri(this.Request.Url, model.RedirectUrl), "alert", "loggedin").ToString());
            return RedirectToAction("Home", "Browse", new { alert = "loggedin" });
        }

        /// <summary>
        /// Displays the user registration page.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            // Check the model is valid.
            if (ModelState.IsValid == false)
            {
                PopulateRegisterViewModel(model);
                return View(model);
            }

            // Trim the text fields.
            model.EmailAddress = model.EmailAddress.Trim();

            // Check an account with the email doesn't already exist.
            string alert;
            bool registered = false;
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
                user = Register(model.EmailAddress, model.Password, model.RegionId);
                registered = true;

                // Alert the user that they have registered successfully.
                alert = "registered";
            }
            this.DataContext.SubmitChanges();

            // Send the user an email if they registered.
            if (registered)
                SendRegistrationEmail(user, model.Password, lawyer: false, askedQuestion: false);

            // Log in as that user.
            Login(user, rememberMe: true);

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

        /// <summary>
        /// Displays the lawyer registration page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult LawyerRegister()
        {
            var model = new LawyerRegisterViewModel();
            model.RegionId = 2;
            PopulateLawyerRegisterViewModel(model);
            return View(model);
        }

        /// <summary>
        /// Registers a new lawyer.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LawyerRegister(LawyerRegisterViewModel model)
        {
            // Check the model is valid.
            if (ModelState.IsValid == false)
            {
                PopulateLawyerRegisterViewModel(model);
                return View(model);
            }

            // Trim the text fields.
            model.EmailAddress = model.EmailAddress.Trim();
            model.FirstName = model.FirstName.Trim();
            model.LastName = model.LastName.Trim();
            if (model.EmployerName != null)
                model.EmployerName = model.EmployerName.Trim();

            var user = this.DataContext.Users.FirstOrDefault(u => u.EmailAddress == model.EmailAddress);
            bool registered = false;
            if (user != null)
            {
                // The user already exists.
                // Check the password is okay and that the user isn't already a lawyer.
                if (BCrypt.Net.BCrypt.Verify(model.Password, user.Password) == false || user.Lawyers.Any())
                {
                    ModelState.AddModelError("EmailAddress", "That email address is already registered.");
                    PopulateLawyerRegisterViewModel(model);
                    return View(model);
                }
            }
            else
            {
                // Register a new user.
                user = Register(model.EmailAddress, model.Password, model.RegionId);
                registered = true;
            }

            // Register a new lawyer.
            var lawyer = new Lawyer();
            lawyer.CreatedOn = DateTimeOffset.Now;
            lawyer.User = user;
            lawyer.FirstName = model.FirstName;
            lawyer.LastName = model.LastName;
            lawyer.YearOfAdmission = model.YearAdmitted;
            lawyer.SpecialisationCategoryId = model.SpecialisationCategoryId == 0 ? (int?)null : model.SpecialisationCategoryId;
            lawyer.EmployerName = model.EmployerName;
            lawyer.Approved = false;
            this.DataContext.Lawyers.InsertOnSubmit(lawyer);

            // Save.
            this.DataContext.SubmitChanges();

            // Send the user a registration email if they registered.
            if (registered)
                SendRegistrationEmail(user, model.Password, lawyer: true, askedQuestion: false);

            // Log in as the new user.
            Login(user, rememberMe: true);

            // Redirect to thank you page.
            return RedirectToAction("LawyerThankYou", new { emailAddress = user.EmailAddress, registered = registered });
        }

        /// <summary>
        /// Displays a thank you page to lawyers that have registered.
        /// </summary>
        /// <param name="emailAddress"> The email address of the lawyer. </param>
        /// <param name="registered"> <c>true</c> if the user account was registered. </param>
        /// <returns></returns>
        public ActionResult LawyerThankYou(string emailAddress, bool registered)
        {
            var model = new LawyerThankYouViewModel();
            model.EmailAddress = emailAddress;
            model.Registered = registered;
            return View(model);
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
        /// Logs the user out.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Logout()
        {
            // Remove the authentication cookie.
            FormsAuthentication.SignOut();

            // Redirect to the home page.
            return RedirectToAction("Home", "Browse", new { alert = "loggedout" });
        }

        /// <summary>
        /// Validates the users email address.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ValidateEmailAddress(int? userId, string token)
        {
            if (userId == null)
                return View("ValidateEmailAddressFailure");

            // Get the users details.
            var user = this.DataContext.Users.SingleOrDefault(u => u.UserId == userId.Value);
            if (user == null)
                return View("ValidateEmailAddressFailure");

            // Check the user hasn't already validated their email address.
            if (user.EmailValidated == true)
                return View("ValidateEmailAddressSuccess");

            // Check the token is valid.
            if (user.EmailValidationToken != token)
                return View("ValidateEmailAddressFailure");

            // Mark the account as validated.
            user.EmailValidated = true;
            user.EmailValidationToken = null;
            this.DataContext.SubmitChanges();

            // Success!
            return View("ValidateEmailAddressSuccess");
        }
    }
}
