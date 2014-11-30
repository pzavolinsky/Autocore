// The MIT License (MIT)
// 
// Copyright (c) 2014 Patricio Zavolinsky
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
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
	/// <summary>
	/// Factory façade.
	/// </summary>
	public static class Factory
	{
		/// <summary>
		/// Registers Autocore into the WepApi configuration loading dependencies from the specified assemblies.
		/// </summary>
		public static IContainer RegisterAutocore(this HttpConfiguration config, params Assembly[] assemblies)
		{
			return config.RegisterAutocore((IEnumerable<Assembly>)assemblies);
		}

		/// <summary>
		/// Registers Autocore into the WepApi configuration loading dependencies from the specified assemblies.
		/// </summary>
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

