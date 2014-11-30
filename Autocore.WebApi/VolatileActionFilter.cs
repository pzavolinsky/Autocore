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
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Autocore.WebApi
{
	/// <summary>
	/// WebApi action filter that wraps action executions in a volatile scope.
	/// </summary>
	public class VolatileActionFilter : ActionFilterAttribute, ISingletonDependency
	{
		/// <summary>
		/// Request context key that stores the volatile scope.
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
		/// Occurs before the action method is invoked.
		/// </summary>
		/// <param name="actionContext">The action context.</param>
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			Push(actionContext);
			base.OnActionExecuting(actionContext);
		}

		/// <summary>
		/// Raises the action executing async event.
		/// </summary>
		/// <param name="actionContext">Action context.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
		{
			Push(actionContext);
			return base.OnActionExecutingAsync(actionContext, cancellationToken);
		}

		/// <summary>
		/// Occurs after the action method is invoked.
		/// </summary>
		/// <param name="actionExecutedContext">The action executed context.</param>
		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			Pop(actionExecutedContext.ActionContext);
			base.OnActionExecuted(actionExecutedContext);
		}

		/// <summary>
		/// Raises the action executed async event.
		/// </summary>
		/// <param name="actionExecutedContext">Action executed context.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
		{
			Pop(actionExecutedContext.ActionContext);
			return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
		}

		/// <summary>
		/// Pushes a new volatile scope in the current call context.
		/// </summary>
		/// <param name="actionContext">Action context.</param>
		protected void Push(HttpActionContext actionContext)
		{
			Stack<ImplicitVolatileScope> stack;
			if (!actionContext.Request.Properties.ContainsKey(PROPERTY_KEY))
			{
				stack = new Stack<ImplicitVolatileScope>();
				actionContext.Request.Properties[PROPERTY_KEY] = stack;
			}
			else
			{
				stack = actionContext.Request.Properties[PROPERTY_KEY] as Stack<ImplicitVolatileScope>;
			}
			stack.Push(new ImplicitVolatileScope(_container));
		}

		/// <summary>
		/// Pops and disposes the last volatile scope in the current call context.
		/// </summary>
		/// <param name="actionContext">Action context.</param>
		protected void Pop(HttpActionContext actionContext)
		{
			if (!actionContext.Request.Properties.ContainsKey(PROPERTY_KEY))
			{
				return;
			}
			var stack = actionContext.Request.Properties[PROPERTY_KEY] as Stack<ImplicitVolatileScope>;
			stack.Pop().Dispose();
		}
	}
}

