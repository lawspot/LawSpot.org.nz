using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Lawspot.Backend;
using Lawspot.Shared;

namespace Lawspot.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// Gets a reference to the EF database context.
        /// </summary>
        public DataClassesDataContext DataContext
        {
            get
            {
                var dataContext = (LawspotDataContext)this.HttpContext.Items["DataContext"];
                if (dataContext == null)
                {
                    dataContext = new LawspotDataContext();
                    this.HttpContext.Items["DataContext"] = dataContext;
                }
                return dataContext;
            }
        }

        /// <summary>
        /// Disposes of any resources used by the current data context.
        /// </summary>
        public static void DisposeDataContext()
        {
            var dataContext = (LawspotDataContext)System.Web.HttpContext.Current.Items["DataContext"];
            if (dataContext != null)
            {
                dataContext.Dispose();
                System.Web.HttpContext.Current.Items["DataContext"] = null;
            }
        }

        /// <summary>
        /// Overrides the User property for easier access.
        /// </summary>
        public new CustomPrincipal User
        {
            get { return base.User as CustomPrincipal; }
        }
    }
}