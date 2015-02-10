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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autocore.Interfaces;

namespace Autocore.WebApi
{
    /// <summary>
    /// WebApi message handler that wraps action execution in a volatile scope.
    /// </summary>
    public class VolatileHandler : DelegatingHandler, ISingletonDependency
    {
        /// <summary>
        /// A listener that can configure volatile context into the current volatile scope.
        /// </summary>
        public interface IListener : ISingletonDependency
        {
            /// <summary>
            /// Configures a volatile container with information extracted from an HTTP action context.
            /// </summary>
            /// <param name="container">The volatile container.</param>
            /// <param name="request">The current HTTP request message.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            Task Configure(IVolatileContainer container, HttpRequestMessage request, CancellationToken cancellationToken);
        }

        private readonly IContainer _container;
        private readonly IEnumerable<IListener> _listeners;

        /// <summary>
        /// Initializes a new volatile action filter instance.
        /// </summary>
        /// <param name="container">Container.</param>
        /// <param name="listeners">A list of volatile scope listeners</param>
        public VolatileHandler(IContainer container, IEnumerable<IListener> listeners)
        {
            _container = container;
            _listeners = listeners;
        }

        /// <see cref="DelegatingHandler.SendAsync"/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await _container.ExecuteInVolatileScopeAsync(async scope =>
            {
                foreach (var listener in _listeners)
                {
                    await listener.Configure(scope, request, cancellationToken);
                }
                return await base.SendAsync(request, cancellationToken);
            });
        }
    }
}
