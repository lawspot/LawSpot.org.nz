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
        public ActionResult Ask()
        {
            var model = new QuestionViewModel();
            PopulateQuestionViewModel(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult Login(QuestionViewModel model)
        {
            // Check the model is valid.
            if (ModelState.IsValid == false)
                return View(model);

            // Redirect to home page.
            return RedirectToAction("Index", "Home", new { alert = "loggedin" });
        }

        /// <summary>
        /// Populate model information that isn't included in the POST data.
        /// </summary>
        /// <param name="model"></param>
        private void PopulateQuestionViewModel(QuestionViewModel model)
        {
            model.Categories = this.DataContext.Categories.ToList().Select(c => new SelectListItem()
            {
                Text = c.Name,
                Value = c.CategoryId.ToString(),
                Selected = model.CategoryId == c.CategoryId
            });
        }
    }
}
