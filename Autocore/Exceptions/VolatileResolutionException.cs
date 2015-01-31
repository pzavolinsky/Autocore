using System;

namespace Autocore
{
	/// <summary>
	/// Volatile resolution exception. Thrown when trying to access a volatile
	/// dependency outside a volatile scope.
	/// 
	/// Note that there are two possible scenarios that throw this exception:
	/// 
	/// 1) Accessing the Value property of a Volatile&lt;T&gt; on the constructor
	/// of a singleton class.
	/// <example>
	/// public interface IVolatileService : IVolatileDependency {}
	/// public class VolatileService : IVolatileService {}
	/// public class UsesVolatile : ISingletonDependency
	/// {
	/// 	private IVolatileService _svc; // wrong!
	/// 	public UsesVolatile(IVolatile&lt;IVolatileService&gt; svc)
	/// 	{
	/// 		_svc = svc.Value; // throws
	///  	}
	/// }
	/// ...
	/// var container = Autocore.Factory.Create();
	/// container.Resolve&lt;UsesVolatile&gt;(); // throws
	/// </example>
	/// 
	/// 2) Leaking a volatile access operation outside a volatile scope. This
	/// usually happens with methods using volatiles that return a Linq
	/// expression that, when executed, access a volatile.
	/// <example>
	/// public interface IVolatileService : IVolatileDependency
	/// {
	/// 	string Get(int num);
	/// }
	/// public class VolatileService : IVolatileService
	/// {
	/// 	public string Get(int num) { return num.ToString(); }
	/// }
	/// public class UsesVolatile : ISingletonDependency
	/// {
	/// 	private IVolatile&lt;IVolatileService&gt; _svc;
	/// 	public UsesVolatile(IVolatile&lt;IVolatileService&gt; svc) { _svc = svc; }
	/// 	public IEnumerable&lt;string&gt; Access()
	/// 	{
	/// 		// The lambda expression in Select is evaluated after
	/// 		// the method returns (and potentially, after the
	/// 		// volatile scope was disposed).
	/// 		return (new[] { 1 }).Select(i => _svc.Value.Get(i));
	/// 	}
	/// }
	/// ...
	/// var container = Autocore.Factory.Create();
	/// var client = container.Resolve&lt;UsesVolatile&gt;();
	/// var list = container.ExecuteInVolatileScope((scope) => client.Access());
	/// list.ToArray(); // throws (evaluates lambda outside the volatile scope)
	/// </example>
	/// 
	/// </summary>
	public class VolatileResolutionException : InvalidOperationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Autocore.VolatileResolutionException"/> class.
		/// </summary>
		public VolatileResolutionException()
			: base ("Attempted to access a volatile dependency outside a volatile scope. Consider wrapping this code in a call to IContainer.ExecuteInVolatileScope")
		{}
	}
}

