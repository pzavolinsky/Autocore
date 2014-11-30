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
using Autocore.Implementation;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Autocore.Mvc
{
	/// <summary>
	/// MVC action filter that wraps controller action execution in a volatile scope.
	/// </summary>
	public class VolatileActionFilter : IActionFilter, ISingletonDependency
	{
		/// <summary>
		/// HttpContext key that stores the volatile scope.
		/// </summary>
		public const string PROPERTY_KEY = "__autocore_scope__";

		IContainer _container;

		/// <summary>
		/// Initializes a new volatile action filter instance.
		/// </summary>
		/// <param name="container">Container.</param>
		public VolatileActionFilter(IContainer container)
		{
			_container = container;
		}

		/// <summary>
		/// Called before an action method executes.
		/// </summary>
		/// <param name="filterContext">The filter context.</param>
		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			Push(filterContext.HttpContext);
		}

		/// <summary>
		/// Called after the action method executes.
		/// </summary>
		/// <param name="filterContext">The filter context.</param>
		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			Pop(filterContext.HttpContext);
		}

		/// <summary>
		/// Pushes a new volatile scope in the current call context.
		/// </summary>
		/// <param name="context">Context.</param>
		protected void Push(HttpContextBase context)
		{
			Stack<ImplicitVolatileScope> stack;
			if (!context.Items.Contains(PROPERTY_KEY))
			{
				stack = new Stack<ImplicitVolatileScope>();
				context.Items[PROPERTY_KEY] = stack;
			}
			else
			{
				stack = context.Items[PROPERTY_KEY] as Stack<ImplicitVolatileScope>;
			}
			stack.Push(new ImplicitVolatileScope(_container));
		}

		/// <summary>
		/// Pops and disposes the last volatile scope in the current call context.
		/// </summary>
		/// <param name="context">Context.</param>
		protected void Pop(HttpContextBase context)
		{
			if (!context.Items.Contains(PROPERTY_KEY))
			{
				return;
			}
			var stack = context.Items[PROPERTY_KEY] as Stack<ImplicitVolatileScope>;
			stack.Pop().Dispose();
		}
	}
}

