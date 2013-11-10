using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TrelloCFD.Website
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Contact",
                url: "Contact",
                defaults: new { controller = "Home", action = "Contact" }
            );

            routes.MapRoute(
                name: "About",
                url: "About",
                defaults: new { controller = "Home", action = "About" }
            );

            routes.MapRoute(
                name: "IdeasAndBugs",
                url: "Ideas-And-Bugs",
                defaults: new { controller = "Home", action = "IdeasAndBugs" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}