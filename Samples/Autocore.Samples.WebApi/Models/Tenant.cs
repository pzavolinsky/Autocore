using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Autocore.Samples.WebApi.Models
{
    public class Tenant : IVolatileDependency
    {
        public string Name { get; set; }
    }
}