using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using Lawspot.Backend;
using Lawspot.Shared;

namespace Lawspot.Controllers
{
    /// <summary>
    /// The base class of all controllers utilizing the Mustache view engine.
    /// </summary>
    public class MustacheController : Controller
    {
        private class EmptyModel
        {
        }

        /// <summary>
        /// Gets a model object of a given type.
        /// </summary>
        /// <param name="viewContext"> The view context. </param>
        /// <param name="modelType"> The type of model to return. </param>
        /// <returns> A model of the given type. </returns>
        protected internal virtual object GetModel(ViewContext viewContext, Type modelType)
        {
            if (viewContext == null)
                throw new ArgumentNullException("viewContext");
            object model = viewContext.ViewData.Model;
            if (modelType == null)
            {
                if (model != null)
                    throw new InvalidOperationException("Expected null model but model was non-null.");
                return new EmptyModel();
            }
            if (modelType != null && model == null)
                throw new InvalidOperationException("Expected model but no model was provided.");
            if (modelType != model.GetType())
                throw new InvalidOperationException(string.Format("Expected model of type {0} but model was of type {1}.", modelType, model.GetType()));
            return model;
        }
    }
}