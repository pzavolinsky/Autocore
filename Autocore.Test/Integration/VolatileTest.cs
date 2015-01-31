using System;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

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
			public HackishVolatileClient(IVolatile<IVolatileService> svc) { if (svc.Value != null) {} }
		}
		public class LeakingVolatileClient : ISingletonDependency
		{
			IVolatile<IVolatileService> _svc;
			public LeakingVolatileClient(IVolatile<IVolatileService> svc) { _svc = svc; }
			public IEnumerable<IVolatileService> Access()
			{
				return new[] { 1 }.Select(i => _svc.Value);
			}
		}
		public class MyVolatileClient : ISingletonDependency
		{
			IVolatile<IVolatileService> _svc;
			public MyVolatileClient(IVolatile<IVolatileService> svc) { _svc = svc; }
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
			Assert.Throws<VolatileResolutionException>(() => client.Access());
		}

		[Test]
		public void SingletonLeaksVolatileOutsideVolatileScope()
		{
			var client = _root.Resolve<LeakingVolatileClient>();
			var list = _root.ExecuteInVolatileScope((scope) => client.Access());
			Assert.Throws<VolatileResolutionException>(() => list.ToArray());
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

