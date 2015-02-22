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
using Autocore.Implementation;
using Autofac;
using Autofac.Integration.Mvc;

namespace Autocore.Mvc
{
	/// <summary>
	/// Volatile scope provider.
	/// </summary>
	public class VolatileScopeProvider : RequestLifetimeScopeProvider
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Autocore.Mvc.VolatileScopeProvider"/> class.
		/// </summary>
		/// <param name="container">Container.</param>
		public VolatileScopeProvider(ILifetimeScope container) : base(container) {}

		/// <see cref="RequestLifetimeScopeProvider.GetLifetimeScopeCore"/> class.
		protected override ILifetimeScope GetLifetimeScopeCore(Action<ContainerBuilder> configurationAction)
		{
			var scope = base.GetLifetimeScopeCore(configurationAction);
			var vs = (Autocore.Implementation.VolatileContainer) new Autocore.Implementation.Container(scope).CreateVolatileScope();
			vs.Resolve<ImplicitContext>().Container = vs;
			return vs.Scope;
		}
	}
}

