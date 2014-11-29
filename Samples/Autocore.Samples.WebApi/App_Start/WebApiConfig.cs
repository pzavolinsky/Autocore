using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Autocore.WebApi;

namespace Autocore.Samples.WebApi
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			// Web API configuration and services
			config.RegisterAutocore(typeof(WebApiConfig).Assembly);

			// Web API routes
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "{controller}/{id}",
				defaults: new { controller = "home", id = "index" }
			);
		}
	}
}
