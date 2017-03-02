using System;
using System.Web.Mvc;

namespace ExigoAdmin.Web.Filters
{
    //Added by Elliott Q. written by Travis W. ||| Made to override ValidateAntiForgeryTokenOnPostAttribute if wanted
    [AttributeUsage(AttributeTargets.All)]
    public class ExternalHttpPostAttribute : AuthorizeAttribute
    {
    }
}