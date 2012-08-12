using System;
using System.Reflection;
using System.Web.Mvc;

/// <summary>
/// When applied to an action method, calls the method when a parameter with the given key
/// matches the given value.
/// </summary>
public class FormSelectorAttribute : ActionMethodSelectorAttribute
{
    public FormSelectorAttribute(string key, string value)
    {
        if (key == null)
            throw new ArgumentNullException("key");
        if (value == null)
            throw new ArgumentNullException("value");
        this.Key = key;
        this.Value = value;
    }

    /// <summary>
    /// The form key that must be present to select the method.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The value that the form key must have in order to select the method.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Determines whether the action method selection is valid for the specified controller context.
    /// </summary>
    /// <param name="controllerContext"> The controller context. </param>
    /// <param name="methodInfo"> Information about the action method. </param>
    /// <returns> <c>true</c> if the action method selection is valid for the specified
    /// controller context; otherwise, <c>false</c>. </returns>
    public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
    {
        var formValue = controllerContext.HttpContext.Request.Form[this.Key];
        if (formValue == this.Value)
            return true;
        return false;
    }
}