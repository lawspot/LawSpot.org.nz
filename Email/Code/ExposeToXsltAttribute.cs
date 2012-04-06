using System;

namespace LawSpot.Email
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ExposeToXsltAttribute : Attribute
    {
    }

}