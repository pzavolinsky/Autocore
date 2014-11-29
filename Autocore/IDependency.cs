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
	/// Internal injectable dependency
	/// </summary>
	public interface IDependency {}
	/// <summary>
	/// Internal injectable non-volatile dependency
	/// </summary>
	public interface INonVolatileDependency : IDependency {}

	/// <summary>
	/// Injectable per-instance dependency.
	/// </summary>
	/// <remarks>
	/// Services implementing this interface will be instantiated every time
	/// the service is resolved though any of the Resolve&lt;T&gt; methods.
	/// That is, the following assertion is always true:
	///   Assert.AreNotEqual(cont.Resolve&lt;IMySvc&gt;(), cont.Resolve&lt;IMySvc&gt;());
	/// </remarks>
	public interface IInstanceDependency  : INonVolatileDependency {}

	/// <summary>
	/// Injectable singleton dependency.
	/// </summary>
	/// <remarks>
	/// Services implementing this interface will be instantiated once per root
	/// container and the same instance will be returned in every call to Resolve&lt;T&gt;.
	/// </remarks>
	public interface ISingletonDependency : INonVolatileDependency {}

	/// <summary>
	/// Injectable volatile dependency.
	/// </summary>
	/// <remarks>
	/// Services implementing this interface will be instantiated once per volatile
	/// scope container and the same instance will be returned in every call to Resolve&lt;T&gt;
	/// inside that volatile scope.
	/// </remarks>
	/// <seealso cref="Autocore.Volatile&lt;T&gt;"/>
	public interface IVolatileDependency  : IDependency {}
}
