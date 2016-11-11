using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.ModelBinding;

namespace Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            
            //Add JWT Authentication Handler
            config.MessageHandlers.Add(new AuthenticationHandler());

            //Require Authentication for All Controllers
            config.Filters.Add(new AuthorizeAttribute());
            
            //Validate All Input Models
            config.Filters.Add(new ValidateModelAttribute());            
        }
    }
}



