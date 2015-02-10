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
using System.Web.Http.Dependencies;
using Autocore.Implementation;
using Autocore.Interfaces;
using Autofac;
using Autofac.Integration.WebApi;

namespace Autocore.WebApi
{
    /// <summary>
    /// WebApi dependency resolver that composes the Autofac dependency resolver and ensure that
    /// .InstancePreRequest scopes are also volatile scopes.
    /// Note: this resolver *requires* adding the VolatileHandler as well (see Factory.RegisterAutocore).
    /// </summary>
    public class DependencyResolver : IDependencyResolver, ISingletonDependency
    {
        private readonly IImplicitContext _ctx;
        private readonly AutofacWebApiDependencyResolver _resolver;

        /// <summary>
        /// Creates a composite dependency resolver using the specified lifetime scope and a volatile
        /// implicit context.
        /// Note: to set this resolver resolve dependencies directly from the container as in Factory.RegisterAutocore.
        /// </summary>
        /// <param name="scope">The Autofac root container scope.</param>
        /// <param name="ctx">The volatile implicit context.</param>
        public DependencyResolver(ILifetimeScope scope, IImplicitContext ctx)
        {
            _ctx = ctx;
            _resolver = new AutofacWebApiDependencyResolver(scope);
        }

        /// <summary>
        /// Disposes the composite resolver.
        /// </summary>
        public void Dispose()
        {
            _resolver.Dispose();
        }

        /// <see cref="IDependencyResolver.GetService"/>
        public object GetService(Type serviceType)
        {
            return _resolver.GetService(serviceType);
        }

        /// <see cref="IDependencyResolver.GetServices"/>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _resolver.GetServices(serviceType);
        }

        /// <see cref="IDependencyResolver.BeginScope"/>
        public IDependencyScope BeginScope()
        {
            var scope = ((VolatileContainer) _ctx.Container).Scope;
            return new AutofacWebApiDependencyResolver(scope).BeginScope();
        }
    }
}
