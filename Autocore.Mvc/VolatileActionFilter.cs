using Autocore.Implementation;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Autocore.Mvc
{
	public class VolatileActionFilter : IActionFilter, ISingletonDependency
	{
		public const string PROPERTY_KEY = "__autocore_scope__";

		IContainer _container;

		public VolatileActionFilter(IContainer container)
		{
			_container = container;
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			Push(filterContext.HttpContext);
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			Pop(filterContext.HttpContext);
		}

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

