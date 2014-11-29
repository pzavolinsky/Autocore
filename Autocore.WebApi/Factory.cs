using System;
using System.Collections.Generic;
using System.Reflection;
using Autocore.Implementation;
using Autofac;
using Autofac.Integration.WebApi;
using System.Linq;
using System.Web.Http;

namespace Autocore.WebApi
{
	public static class Factory
	{
		public static IContainer RegisterAutocore(this HttpConfiguration config, params Assembly[] assemblies)
		{
			return config.RegisterAutocore((IEnumerable<Assembly>)assemblies);
		}
		public static IContainer RegisterAutocore(this HttpConfiguration config, IEnumerable<Assembly> assemblies)
		{
			var asm = typeof(Factory).Assembly;
			if (!assemblies.Contains(asm))
			{
				assemblies = assemblies.Concat(new Assembly[] { asm });
			}
			var builder = new Autofac.ContainerBuilder();
			builder.RegisterDependencyAssemblies(assemblies);
			builder.RegisterApiControllers(assemblies.ToArray());
			var scope = builder.Build();
			var container = new Autocore.Implementation.Container(scope);
			config.DependencyResolver = new AutofacWebApiDependencyResolver(scope);
			config.Filters.Add(container.Resolve<VolatileActionFilter>());
			return container;
		}
	}
}

