using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Lawspot.Backend;
using Lawspot.Views.Ask;

namespace Lawspot.Controllers
{
    public class AskController : BaseController
    {
        [HttpGet]
        public ActionResult Ask(string title, int? category)
        {
            var model = new QuestionViewModel();
            model.Title = title;
            if (category.HasValue)
                model.CategoryId = category.Value;
            PopulateQuestionViewModel(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult Ask(QuestionViewModel model)
        {
            if (model.Registration != null)
            {
                // We don't have another agreement checkbox for the registration.
                ModelState.Remove("Registration.Agreement");
            }

            // Remove all registration validation errors if the login form is showing.
            // And vice-versa.
            var keysToRemove = new List<string>();
            foreach (var key in ModelState.Keys)
            {
                if (model.ShowRegistration == false && key.StartsWith("Registration."))
                    keysToRemove.Add(key);
                if (model.ShowLogin == false && key.StartsWith("Login."))
                    keysToRemove.Add(key);
            }
            foreach (var key in keysToRemove)
                ModelState.Remove(key);

            // Check the model is valid.
            if (ModelState.IsValid == false)
            {
                PopulateQuestionViewModel(model);
                return View(model);
            }

            // Ask the user to register or log in if they haven't already.
            if (model.Registration == null && this.User == null)
            {
                model.Registration = new Views.Account.RegisterViewModel();
                model.Registration.RegionId = 2;
                model.Login = new Views.Account.LoginViewModel();
                model.Login.RememberMe = true;
                model.ShowRegistration = true;
                PopulateQuestionViewModel(model);
                return View(model);
            }

            User user;
            if (this.User == null)
            {
                if (model.ShowRegistration)
                {
                    // Check an account with the email doesn't already exist.
                    user = this.DataContext.Users.FirstOrDefault(u => u.EmailAddress == model.Registration.EmailAddress);
                    if (user != null)
                    {
                        // Check the password is okay.
                        if (BCrypt.Net.BCrypt.Verify(model.Registration.Password, user.Password) == false)
                        {
                            ModelState.AddModelError("Registration.EmailAddress", "That email address is already registered.");
                            PopulateQuestionViewModel(model);
                            return View(model);
                        }

                        // The user tried to register, but they got the email address and password
                        // right, so we'll just log them in.
                        user.RegionId = model.Registration.RegionId;
                    }
                    else
                    {
                        // Register a new user.
                        user = new User();
                        user.EmailAddress = model.Registration.EmailAddress;
                        user.Password = BCrypt.Net.BCrypt.HashPassword(model.Registration.Password, workFactor: 12);
                        user.RegionId = model.Registration.RegionId;
                        this.DataContext.Users.InsertOnSubmit(user);
                    }
                }
                else
                {
                    // Get the user with the given email address and password (if any).
                    user = this.DataContext.Users.Where(u => u.EmailAddress == model.Login.EmailAddress).FirstOrDefault();
                    if (user == null || BCrypt.Net.BCrypt.Verify(model.Login.Password, user.Password) == false)
                    {
                        ModelState.AddModelError("Login.Password", "The email or password you entered is incorrect.");
                        PopulateQuestionViewModel(model);
                        return View(model);
                    }
                }

                // Log in as that user.
                Login(user, rememberMe: model.Login.RememberMe);
            }
            else
            {
                // Get the details of the logged in user.
                user = this.DataContext.Users.Where(u => u.EmailAddress == this.User.EmailAddress).Single();
            }

            // Submit a new question.
            var question = new Question();
            question.Title = model.Title;
            question.Details = model.Details;
            question.CategoryId = model.CategoryId;
            question.CreatedOn = DateTime.Now;
            question.User = user;
            this.DataContext.Questions.InsertOnSubmit(question);

            // Save changes.
            this.DataContext.SubmitChanges();

            // Redirect to the thank you page.
            return RedirectToAction("ThankYou", new { questionId = question.QuestionId });
        }

        /// <summary>
        /// Populate model information that isn't included in the POST data.
        /// </summary>
        /// <param name="model"></param>
        private void PopulateQuestionViewModel(QuestionViewModel model)
        {
            model.Categories = this.DataContext.Categories.Select(c => new SelectListItem()
            {
                Text = c.Name,
                Value = c.CategoryId.ToString(),
                Selected = model.CategoryId == c.CategoryId
            });

            if (model.Registration == null)
            {
                model.FocusInTitle = string.IsNullOrEmpty(model.Title);
                model.FocusInDetails = !model.FocusInTitle;
            }
            else
            {
                if (model.ShowRegistration == true)
                    model.FocusInRegistrationEmailAddress = true;
                else
                    model.FocusInLoginEmailAddress = true;
                model.Registration.Regions = this.DataContext.Regions.Select(r => new SelectListItem()
                {
                    Value = r.RegionId.ToString(),
                    Text = r.Name,
                    Selected = model.Registration.RegionId == r.RegionId,
                });
            }
        }

        [HttpGet]
        public ActionResult ThankYou(int questionId)
        {
            // Load the question from the database.
            var question = this.DataContext.Questions.Where(q => q.QuestionId == questionId).Single();

            // Populate a model and show the view.
            var model = new QuestionThankModel();
            model.Title = question.Title;
            model.Category = question.Category.Name;
            model.EmailAddress = question.User.EmailAddress;
            return View(model);
        }
    }
}
