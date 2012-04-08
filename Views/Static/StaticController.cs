using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Lawspot.Backend;
using Lawspot.Views.Admin;

namespace Lawspot.Controllers
{
    public class StaticController : BaseController
    {
        public ActionResult PartnerWithUs()
        {
            return View();
        }

        public ActionResult ContactUs()
        {
            return View();
        }
    }
}
