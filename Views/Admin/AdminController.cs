using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Lawspot.Backend;
using Lawspot.Views.Admin;

namespace Lawspot.Controllers
{
    public class AdminController : BaseController
    {
        /// <summary>
        /// All methods on this controller require that the logged in user be a volunteer admin or
        /// a CLC lawyer.
        /// </summary>
        /// <param name="filterContext"> The request context. </param>
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (this.User == null)
                filterContext.Result = new HttpUnauthorizedResult();
            else if (this.User.IsVolunteerAdmin == false && this.User.IsCLCLawyer == false)
                filterContext.Result = new HttpStatusCodeResult(403);
        }

        /// <summary>
        /// Displays the answer questions page.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ApproveLawyers(string category, string filter, string sort)
        {
            var model = new ApproveLawyersViewModel();

            // Categories.
            int categoryId = 0;
            if (category != null)
                categoryId = int.Parse(category);
            model.CategoryOptions = new SelectListItem[] {
                    new SelectListItem()
                    {
                        Text = "All Areas Of Expertise",
                        Value = "0",
                        Selected = categoryId == 0,
                    }
                }.Union(
                    this.DataContext.Categories
                    .OrderBy(c => c.Name)
                    .ToList()
                    .Select(c => new SelectListItem()
                    {
                        Text = c.Name,
                        Value = c.CategoryId.ToString(),
                        Selected = c.CategoryId == categoryId,
                    }));

            // Filter.
            var filterValue = LawyerFilter.NotYetApproved;
            if (filter != null)
                filterValue = (LawyerFilter)Enum.Parse(typeof(LawyerFilter), filter, true);
            model.FilterOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "Not Yet Approved", Value = LawyerFilter.NotYetApproved.ToString(), Selected = filterValue == LawyerFilter.NotYetApproved },
                new SelectListItem() { Text = "Approved", Value = LawyerFilter.Approved.ToString(), Selected = filterValue == LawyerFilter.Approved },
            };

            // Sort order.
            var sortValue = LawyerSortOrder.MostRecent;
            if (sort != null)
                sortValue = (LawyerSortOrder)Enum.Parse(typeof(LawyerSortOrder), sort, true);
            model.SortOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "First To Register", Value = LawyerSortOrder.FirstToRegister.ToString(), Selected = sortValue == LawyerSortOrder.FirstToRegister },
                new SelectListItem() { Text = "Most Recent", Value = LawyerSortOrder.MostRecent.ToString(), Selected = sortValue == LawyerSortOrder.MostRecent },
            };

            // Filter and sort.
            IEnumerable<Lawyer> lawyers = this.DataContext.Lawyers;
            if (categoryId != 0)
                lawyers = lawyers.Where(l => l.SpecialisationCategoryId == categoryId);
            switch (filterValue)
            {
                case LawyerFilter.Approved:
                    lawyers = lawyers.Where(l => l.Approved == true);
                    break;
                case LawyerFilter.NotYetApproved:
                    lawyers = lawyers.Where(l => l.Approved == false);
                    break;
            }
            switch (sortValue)
            {
                case LawyerSortOrder.FirstToRegister:
                    lawyers = lawyers.OrderBy(l => l.CreatedOn);
                    break;
                case LawyerSortOrder.MostRecent:
                    lawyers = lawyers.OrderByDescending(l => l.CreatedOn);
                    break;
            }
            model.Lawyers = lawyers
                .ToList()
                .Select(l => new LawyerViewModel()
                {
                    LawyerId = l.LawyerId,
                    Approved = l.Approved,
                    Name = l.FullName,
                    EmailAddress = l.User.EmailAddress,
                    DateRegistered = l.CreatedOn.ToString("d MMM yyyy"),
                    YearAdmitted = l.YearOfAdmission,
                });

            return View(model);
        }
    }
}
