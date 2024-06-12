﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace UMC_Email
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            // Route mới cho Admin
            routes.MapRoute(
                name: "AdminDefault",
                url: "Admin",
                defaults: new { controller = "Admin", action = "Index" }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Email", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
