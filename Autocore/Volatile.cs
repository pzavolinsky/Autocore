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
namespace Autocore
{
	/// <summary>
	/// Singleton volatile accessor.
	/// </summary>
	/// <description>
	/// Volatile dependencies are services that are only valid for a brief period
	/// of time. That is, volatile dependencies exist within a volatile scope.
	/// 
	/// Volatile scopes can be created on a per-execution-context basis (e.g. 
	/// per-request, per-thread, per-async call, etc.) allowing different executions
	/// of singleton instances to see different volatile values.
	/// 
	/// A singleton service that requires access to a volatile service needs to take
	/// a dependency on Volatile<IMyService> instead of directly on IMyService because
	/// the singleton will be called many times with different IMyService instances.
	/// </description>
	/// <remarks>
	/// A singleton's constructor and its methods will be executed in different
	/// execution contexts (i.e. the ctor will be called once during app initialization
	/// and the methods will be called many times to respond to external events). Because
	/// of this a singleton SHOULD NOT use the Value acccessor of a Volatile in its
	/// constructor.
	/// </remarks>
	/// <example>
	/// public interface IHttpBasicAuthValidator : ISingletonDependency
	/// {
	/// 	bool IsValid();
	/// }
	/// public interface IHttpRequest : IVolatileDependency
	/// {
	/// 	string Username { get; }
	/// 	string Password { get; }
	/// }
	/// public class HttpBasicAuthValidator : IHttpBasicAuthValidator
	/// {
	/// 	private Volatile&lt;IHttpRequest&gt; _request;
	/// 	public HttpBasicAuthValidator(Volatile&lt;IHttpRequest&gt; _request)
	/// 	{
	/// 		_request = request;
	/// 	}
	/// 	public bool IsValue()
	/// 	{
	/// 		var rq = _request.Value;
	/// 		return rq.Username == "Test" &amp;&amp; rq.Password == "test";
	/// 	}
	/// }
	/// </example>
	public class Volatile<T> : ISingletonDependency where T : IVolatileDependency
	{
		IVolatileContext _context;

		/// <summary>
		/// Initializes a new instance of the <see cref="Autocore.Volatile`1"/> class.
		/// </summary>
		/// <param name="context">Volatile context.</param>
		public Volatile(IVolatileContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Gets the volatile value.
		/// </summary>
		/// <value>The value.</value>
		/// <remarks>
		/// DO NOT call Value in the constructor of a singleton service! 
		/// </remarks>
		public T Value 
		{
			get { return _context.Resolve<T>(); }
		}
	}
}
