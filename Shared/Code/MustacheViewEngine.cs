using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lawspot.Shared
{
    public class MustacheViewEngine : IViewEngine
    {
        /// <summary>
        /// Finds the specified view by using the specified controller context.
        /// </summary>
        /// <param name="controllerContext"> The controller context. </param>
        /// <param name="viewName"> The name of the view. </param>
        /// <param name="masterName"> The name of the master. </param>
        /// <param name="useCache"> true to specify that the view engine returns the cached view, if a cached view exists; otherwise, false. </param>
        /// <returns> The page view. </returns>
        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            var controllerName = controllerContext.RouteData.GetRequiredString("controller");
            var actionName = controllerContext.RouteData.GetRequiredString("action");
            return new ViewEngineResult(new MustacheView(controllerContext.Controller, controllerName, actionName, viewName), this);
        }

        /// <summary>
        /// Finds the specified partial view by using the specified controller context.
        /// </summary>
        /// <param name="controllerContext"> The controller context. </param>
        /// <param name="partialViewName"> The name of the partial view. </param>
        /// <param name="useCache"> true to specify that the view engine returns the cached view, if a cached view exists; otherwise, false. </param>
        /// <returns> The partial view. </returns>
        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Releases the specified view by using the specified controller context.
        /// </summary>
        /// <param name="controllerContext"> The controller context. </param>
        /// <param name="view"> The view. </param>
        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            if (view is IDisposable)
                ((IDisposable)view).Dispose();
        }
    }
}

