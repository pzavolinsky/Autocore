using Autocore.Implementation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Autocore.WebApi
{
	public class VolatileActionFilter : ActionFilterAttribute, ISingletonDependency
	{
		public const string PROPERTY_KEY = "__autocore_scope__";

		IContainer _container;

		public VolatileActionFilter(IContainer container)
		{
			_container = container;
		}

		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			Push(actionContext);
			base.OnActionExecuting(actionContext);
		}

		public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
		{
			Push(actionContext);
			return base.OnActionExecutingAsync(actionContext, cancellationToken);
		}

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			Pop(actionExecutedContext.ActionContext);
			base.OnActionExecuted(actionExecutedContext);
		}

		public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
		{
			Pop(actionExecutedContext.ActionContext);
			return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
		}

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

