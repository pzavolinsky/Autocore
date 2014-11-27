using System;
using System.Web.Mvc;
using Autocore.Implementation;
using System.Collections.Generic;

namespace Autocore.Mvc
{
	public class VolatileActionFilter : IActionFilter, ISingletonDependency
	{
		Stack<ImplicitVolatileScope> _stack = new Stack<ImplicitVolatileScope>();
		IContainer _container;

		public VolatileActionFilter(IContainer container)
		{
			_container = container;
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			_stack.Push(new ImplicitVolatileScope(_container));
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			_stack.Pop().Dispose();
		}
	}
}

