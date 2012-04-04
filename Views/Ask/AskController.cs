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
                // Check an account with the email doesn't already exist.
                if (this.DataContext.Users.Any(u => u.EmailAddress == model.Registration.EmailAddress))
                    ModelState.AddModelError("Registration.EmailAddress", "That email address is already registered.");

                // We don't have another agreement checkbox for the registration.
                ModelState.Remove("Registration.Agreement");
            }

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
                PopulateQuestionViewModel(model);
                return View(model);
            }

            User loggedInUser;
            if (this.User == null)
            {
                // Register a new user.
                var user = new User();
                user.EmailAddress = model.Registration.EmailAddress;
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Registration.Password, workFactor: 12);
                user.RegionId = model.Registration.RegionId;
                this.DataContext.Users.InsertOnSubmit(user);

                // Log in as that user.
                loggedInUser = user;
                Login(loggedInUser, rememberMe: true);
            }
            else
            {
                // Get the details of the logged in user.
                loggedInUser = this.DataContext.Users.Where(u => u.EmailAddress == this.User.EmailAddress).Single();
            }

            // Submit a new question.
            var question = new Question();
            question.Title = model.Title;
            question.Details = model.Details;
            question.CategoryId = model.CategoryId;
            question.CreatedOn = DateTime.Now;
            question.CreatedByUserId = loggedInUser.UserId;
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
                model.FocusInEmailAddress = true;
                model.Registration.Agreement = true;
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
