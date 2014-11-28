using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using Autocore.Implementation;
using Autofac;
using Autofac.Integration.Mvc;
using System.Linq;

namespace Autocore.Mvc
{
	public static class Factory
	{
		public static IContainer Create(params Assembly[] assemblies)
		{
			return Create((IEnumerable<Assembly>)assemblies);
		}
		public static IContainer Create(IEnumerable<Assembly> assemblies)
		{
			var asm = typeof(Factory).Assembly;
			if (!assemblies.Contains(asm))
			{
				assemblies = assemblies.Concat(new Assembly[] { asm });
			}
			var builder = new Autofac.ContainerBuilder();
			builder.RegisterDependencyAssemblies(assemblies);
			builder.RegisterControllers(assemblies.ToArray());
			var scope = builder.Build();
			var container = new Autocore.Implementation.Container(scope);
			DependencyResolver.SetResolver(new AutofacDependencyResolver(scope));
			GlobalFilters.Filters.Add(container.Resolve<VolatileActionFilter>());
			return container;
		}
	}
}

