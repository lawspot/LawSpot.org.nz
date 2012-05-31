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
        public ActionResult HowItWorks()
        {
            // Activate header tab.
            this.HowItWorksTabActive = true;

            return View();
        }

        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult AboutUs()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult TermsOfUse()
        {
            return View();
        }

        public ActionResult LawyerPolicy()
        {
            return View();
        }

        public ActionResult ImportantNotice()
        {
            return View();
        }

        public ActionResult PartnerWithUs()
        {
            return View();
        }

        public ActionResult ContactUs()
        {
            return View();
        }

        public ActionResult Error404()
        {
            return View("404");
        }

        public ActionResult Error500()
        {
            return View("500");
        }
    }
}
