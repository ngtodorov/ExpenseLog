using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using ExpenseLogWebAPI.Helpers;

namespace ExpenseLogWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Enable CORS (Cross Origin Resource Sharing)
            // https://docs.microsoft.com/en-us/aspnet/web-api/overview/security/enabling-cross-origin-requests-in-web-api
            ExpenseLogCommon.Utils utils = new ExpenseLogCommon.Utils();
            string coreOrigins = utils.GetAppSetting("EL_CORS_ORIGINS");
            var cors = new EnableCorsAttribute(coreOrigins, "*", "*");
            config.EnableCors(cors);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

    }
}
