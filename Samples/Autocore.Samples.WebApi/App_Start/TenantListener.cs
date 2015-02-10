using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autocore.Interfaces;
using Autocore.Samples.WebApi.Models;
using Autocore.WebApi;

namespace Autocore.Samples.WebApi
{
    public class TenantListener : VolatileHandler.IListener
    {
        public Task Configure(IVolatileContainer container, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            container.Resolve<Tenant>().Name = request.GetQueryNameValuePairs()
                .Where(kv => kv.Key == "tenant")
                .Select(kv => kv.Value)
                .FirstOrDefault();

            return Task.FromResult(0);
        }
    }
}