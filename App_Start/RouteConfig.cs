using System.Web.Mvc;
using System.Web.Routing;

namespace MVC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = 0 }
            );

            routes.MapRoute(
               name: "Default1",
               url: "{controller}/{action}/{code}",
               defaults: new { controller = "Home", action = "OnAuthComplate" }
           );
        }
    }
}
