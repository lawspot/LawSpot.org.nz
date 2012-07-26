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
            // Activate header tab.
            this.AskALawyerTabActive = true;

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

            // Trim the text fields.
            model.Title = model.Title.Trim();
            model.Details = model.Details.Trim();
            if (model.Registration != null && model.Registration.EmailAddress != null)
                model.Registration.EmailAddress = model.Registration.EmailAddress.Trim();
            if (model.Login != null && model.Login.EmailAddress != null)
                model.Login.EmailAddress = model.Login.EmailAddress.Trim();

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
            bool registered = false;
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
                        user = Register(model.Registration.EmailAddress, model.Registration.Password, model.Registration.RegionId);
                        registered = true;
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

            // Create a slug for the question.
            var slugBuilder = new System.Text.StringBuilder();
            foreach (var c in model.Title)
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-')
                    slugBuilder.Append(c);
                else if (c >= 'A' && c <= 'Z')
                    slugBuilder.Append(char.ToLowerInvariant(c));
                else if (c == ' ')
                    slugBuilder.Append('-');
            }
            
            // Replace double dashes with a single dash.
            slugBuilder.Replace("--", "-");

            // Trim any dashes at the start and end.
            var slug = slugBuilder.ToString();
            slug = slug.Trim('-');

            if (slug.Length > 66)
            {
                // Limit to 66 characters so there is space for the uniquifier.
                slug = slug.Substring(0, 66);

                // Chop off the last partial word.
                if (slug.LastIndexOf('-') >= 10)
                    slug = slug.Substring(0, slug.LastIndexOf('-'));
            }

            // Ensure the slug is unique.
            if (this.DataContext.Questions.Any(q => q.Slug == slug.ToString()))
            {
                int uniquifier = 2;
                while (this.DataContext.Questions.Any(q => q.Slug == string.Format("{0}-{1}", slug, uniquifier)))
                    uniquifier++;
                slug += uniquifier.ToString();
            }

            // Submit a new question.
            var question = new Question();
            question.Title = model.Title;
            question.Details = model.Details;
            question.CategoryId = model.CategoryId;
            question.CreatedOn = DateTimeOffset.Now;
            question.User = user;
            question.Slug = slug.ToString();
            this.DataContext.Questions.InsertOnSubmit(question);

            // Save changes.
            this.DataContext.SubmitChanges();

            // Send the user a registration email if they registered.
            if (registered)
                SendRegistrationEmail(user, model.Registration.Password, lawyer: false, askedQuestion: true);

            // Redirect to the thank you page.
            return RedirectToAction("ThankYou", new { questionId = question.QuestionId, registered = registered });
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
            }).OrderBy(sli => sli.Text);

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

        /// <summary>
        /// Displays the "thank you for submitting a question" page.
        /// </summary>
        /// <param name="questionId"> The ID of the new question. </param>
        /// <param name="registered"> <c>true</c> if the user registered to ask the question. </param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ThankYou(int questionId, bool registered)
        {
            // Load the question from the database.
            var question = this.DataContext.Questions.Where(q => q.QuestionId == questionId).Single();

            // Populate a model and show the view.
            var model = new QuestionThankYouModel();
            model.Title = question.Title;
            model.Category = question.Category.Name;
            model.EmailAddress = question.User.EmailAddress;
            model.Registered = registered;
            return View(model);
        }

        private class SuggestionsResult
        {
            public string QueryText { get; set; }
            public string MoreUri { get; set; }
            public IEnumerable<SearchResult> Results { get; set; }
        }

        private class SearchResult
        {
            public string Title { get; set; }
            public string Uri { get; set; }
        }

        /// <summary>
        /// Returns search results for the given text.  Used when asking a question.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult Suggestions(string text)
        {
            var result = new SuggestionsResult();
            result.QueryText = text;
            
            // Search.
            var hits = Lawspot.Shared.SearchIndexer.Search(text);
            result.Results = hits.Join(this.DataContext.Questions, hit => hit.ID, q => q.QuestionId, (hit, q) => new SearchResult()
            {
                Title = q.Title,
                Uri = q.AbsolutePath,
            }).ToList();

            // Show a more link if there are more than 5 results.
            if (result.Results.Count() > 5)
            {
                result.Results = result.Results.Take(5);
                result.MoreUri = string.Format("/search?query={0}", Uri.EscapeDataString(text));
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
