using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using Autofac;
using Autocore.Implementation;

namespace Autocore.Mvc
{
	public static class Factory
	{
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

