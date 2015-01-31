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
using Autocore.Interfaces;
using System.Collections.Generic;

namespace Autocore.Implementation
{
	/// <summary>
	/// Volatile context implementation.
	/// </summary>
	public class VolatileContext : IVolatileContext
	{
		IImplicitContext _context;

		/// <summary>
		/// Initializes a new volatile context instance.
		/// </summary>
		/// <param name="context">Implicit context.</param>
		public VolatileContext(IImplicitContext context)
		{
			_context = context;
		}

		/// <see cref="Autocore.Interfaces.IVolatileContext.Resolve&lt;T&gt;"/>
		public T Resolve<T>() where T : IVolatileDependency
		{
			if (_context.Container == null)
			{
				throw new VolatileResolutionException();
			}
			return _context.Container.Resolve<T>();
		}

		/// <see cref="Autocore.Interfaces.IVolatileContext.ResolveEnumerable&lt;T&gt;"/>
		public IEnumerable<T> ResolveEnumerable<T>() where T : IVolatileDependency
		{
			if (_context.Container == null)
			{
				throw new VolatileResolutionException();
			}
			return _context.Container.ResolveEnumerable<T>();
		}

	}
}
