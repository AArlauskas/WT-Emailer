using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;

namespace emailer_backend
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            StripeConfiguration.ApiKey = "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";
            config.Formatters.JsonFormatter.SupportedMediaTypes
            .Add(new MediaTypeHeaderValue("text/html"));
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
