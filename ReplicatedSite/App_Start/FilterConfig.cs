using System.Web.Mvc;

namespace ReplicatedSite
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            Common.Filters.GlobalFilters.Register(filters);
        }
    }
}