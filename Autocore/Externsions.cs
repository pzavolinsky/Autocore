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
using System.Threading.Tasks;

namespace Autocore
{
	public static class Extensions
	{
		/// <summary>
		/// Executes the callback in a volatile scope that is disposed after the execution.
		/// </summary>
		/// <returns>The value returned by callback.</returns>
		/// <param name="container">Container instance.</param>
		/// <param name="callback">Callback function to be executed in a volatile scope.</param>
		/// <typeparam name="T">The return value type of callback.</typeparam>
		public static T ExecuteInVolatileScope<T>(this IContainer container, Func<IVolatileContainer, T> callback)
		{
			using (var scope = new Implementation.ImplicitVolatileScope(container))
			{
				return callback(scope.Container);
			}
		}

		/// <summary>
		/// Executes the async callback in a volatile scope that is disposed after the execution.
		/// </summary>
		/// <returns>The value returned by callback.</returns>
		/// <param name="container">Container instance.</param>
		/// <param name="callback">Async callback function to be executed in a volatile scope.</param>
		/// <typeparam name="T">The return value type of callback.</typeparam>
		public static async Task<T> ExecuteInVolatileScopeAsync<T>(this IContainer container, Func<IVolatileContainer, Task<T>> callback)
		{
			using (var scope = new Implementation.ImplicitVolatileScope(container))
			{
				return await callback(scope.Container);
			}
		}

		/// <summary>
		/// Executes the callback in a volatile scope that is disposed after the execution.
		/// </summary>
		/// <param name="container">Container instance.</param>
		/// <param name="callback">Callback function to be executed in a volatile scope.</param>
		public static void ExecuteInVolatileScope(this IContainer container, Action<IVolatileContainer> callback)
		{
			using (var scope = new Implementation.ImplicitVolatileScope(container))
			{
				callback(scope.Container);
			}
		}

		/// <summary>
		/// Executes the async callback in a volatile scope that is disposed after the execution.
		/// </summary>
		/// <param name="container">Container instance.</param>
		/// <param name="callback">Async callback function to be executed in a volatile scope.</param>
		public static async Task ExecuteInVolatileScopeAsync(this IContainer container, Func<IVolatileContainer, Task> callback)
		{
			using (var scope = new Implementation.ImplicitVolatileScope(container))
			{
				await callback(scope.Container);
			}
		}
	}
}
