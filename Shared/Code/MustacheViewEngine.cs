using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lawspot.Shared
{
    public class MustacheViewEngine : VirtualPathProviderViewEngine
    {
        public MustacheViewEngine()
        {
            // This is where we tell MVC where to look for our files.
            base.ViewLocationFormats = new string[] { "~/Views/{1}/{0}.html", "~/Views/{1}/{0}/{0}.html" };
            base.PartialViewLocationFormats = base.ViewLocationFormats;
        }

        protected override IView CreateView(ControllerContext context, string viewPath, string masterPath)
        {
            return new MustacheView(context.Controller, viewPath);
        }

        protected override IView CreatePartialView(ControllerContext context, string partialPath)
        {
            return new MustacheView(context.Controller, partialPath);
        }
    }
}

