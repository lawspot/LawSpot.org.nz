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
            return View();
        }
    }
}
