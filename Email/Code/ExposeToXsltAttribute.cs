using System;

namespace Lawspot.Email
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ExposeToXsltAttribute : Attribute
    {
    }

}