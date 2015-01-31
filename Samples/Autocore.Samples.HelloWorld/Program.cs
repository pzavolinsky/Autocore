using System;
using System.Linq;
using System.Collections.Generic;

namespace Autocore.Samples.HelloWorld
{
	public interface IVolatileService : IVolatileDependency {}
	public class VolatileService : IVolatileService {}
	public class BreaksVolatile : ISingletonDependency
	{
		public BreaksVolatile(IVolatileService svc) // throws
		{
		}
	}

	public static class Program
	{
		public static void Main(string[] args)
		{
			using (var container = Autocore.Factory.Create())
			{
				container.Resolve<BreaksVolatile>();
			}
		}
	}
}
