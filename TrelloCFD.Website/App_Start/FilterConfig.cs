using System.Web;
using System.Web.Mvc;
using TrelloCFD.Website.Filters;

namespace TrelloCFD.Website
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new TlsAttribute());
            filters.Add(new HandleErrorAttribute());
        }
    }
}