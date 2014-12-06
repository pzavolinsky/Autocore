using System;
using Moq;
using NUnit.Framework;

namespace Autocore.Test.Integration
{
	public class VolatileTest : IntegrationBase
	{
		public interface IVolatileService : IVolatileDependency {}
		public class MyVolatileService : IVolatileService {}
		public class BrokenVolatileClient : ISingletonDependency
		{
			public BrokenVolatileClient(IVolatileService svc) {}
		}
		public class HackishVolatileClient : ISingletonDependency
		{
			public HackishVolatileClient(Volatile<IVolatileService> svc) { if (svc.Value != null) {} }
		}
		public class MyVolatileClient : ISingletonDependency
		{
			Volatile<IVolatileService> _svc;
			public MyVolatileClient(Volatile<IVolatileService> svc) { _svc = svc; }
			public IVolatileService Access() { return _svc.Value; }
		}

		[Test]
		public void SingletonWithDirectVolatileDependency()
		{
			Assert.Throws<Autofac.Core.DependencyResolutionException>(() => _root.Resolve<BrokenVolatileClient>());
		}

		[Test]
		public void SingletonCallsVolatileValueInCtor()
		{
			Assert.Throws<Autofac.Core.DependencyResolutionException>(() => _root.Resolve<HackishVolatileClient>());
		}

		[Test]
		public void SingletonAccessesVolatileOutsideVolatileScope()
		{
			var client = _root.Resolve<MyVolatileClient>();
			Assert.Throws<InvalidOperationException>(() => client.Access());
		}

		[Test]
		public void SingletonAccessesVolatile()
		{
			var client = _root.Resolve<MyVolatileClient>();
			_root.ExecuteInVolatileScope((scope) => client.Access());      // Func<T,U>
			_root.ExecuteInVolatileScope((scope) => { client.Access(); }); // Action<T>
		}
	}
}

