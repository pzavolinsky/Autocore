using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Autocore.Interfaces;
using Autocore.Samples.WebApi.Models;
using Autocore.WebApi;

namespace Autocore.Samples.WebApi
{
    public class TenantListener : VolatileActionFilter.IListener
    {
        public void Configure(IVolatileContainer container, HttpActionContext context)
        {
            container.Resolve<Tenant>().Name = context.Request.GetQueryNameValuePairs()
                .Where(kv => kv.Key == "tenant")
                .Select(kv => kv.Value)
                .FirstOrDefault();
        }
    }
}