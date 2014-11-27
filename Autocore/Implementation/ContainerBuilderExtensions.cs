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
using Autofac;
using Autofac.Builder;
using System.Linq;

namespace Autocore.Implementation
{
	public static class ContainerBuilderExtensions
	{
		public static void RegisterDependencyAssemblies(this ContainerBuilder builder, IEnumerable<Assembly> assemblies)
		{
			var asm = typeof(ContainerBuilderExtensions).Assembly;
			if (!assemblies.Contains(asm))
			{
				assemblies = assemblies.Concat(new Assembly[] { asm });
			}
			RegisterDependencyTypes(builder, assemblies
                .SelectMany(a => a.GetTypes())
				.Where(t => !t.IsAbstract && t.IsClass && _dependency.IsAssignableFrom(t))
            );
		}

		public static void RegisterDependencyTypes(this ContainerBuilder builder, IEnumerable<Type> types)
		{
			foreach (var comp in types)
			{
				if (comp.IsGenericType)
				{
					RegisterComponent(builder.RegisterGeneric(comp).AsSelf(), comp);
				}
				else
				{
					RegisterComponent(builder.RegisterType(comp).AsSelf(), comp);
				}
			}
		}

		public static void RegisterComponent<T, U, V>(IRegistrationBuilder<T,U,V> reg, Type comp)
		{
			foreach (var svc in comp.GetInterfaces().Where(i => _dependency.IsAssignableFrom(i)))
			{
				if (svc != _dependency && 
				    svc != _nonVolatileDependency &&
				    svc != _instanceDependency &&
				    svc != _singletonDependency && 
				    svc != _volatileDependency)
					reg.As(svc);
			}

			if (_singletonDependency.IsAssignableFrom(comp))
			{
				reg.SingleInstance();
			}
			if (_volatileDependency.IsAssignableFrom(comp))
			{
				reg.InstancePerMatchingLifetimeScope(Container.VOLATILE_TAG);
			}
		}

		private static Type _dependency = typeof(IDependency);
		private static Type _nonVolatileDependency = typeof(INonVolatileDependency);
		private static Type _instanceDependency = typeof(IInstanceDependency);
		private static Type _singletonDependency = typeof(ISingletonDependency);
		private static Type _volatileDependency = typeof(IVolatileDependency);
	}
}

