using System;
using Moq;
using NUnit.Framework;

namespace Autocore.Test.Integration
{
	public class SingletonIntegrationTest : IntegrationBase
	{
		public interface ISingletonService : ISingletonDependency {}
		public interface IOtherSingletonService : ISingletonDependency {}
		public class MySingletonService : ISingletonService, IOtherSingletonService {}

		[Test]
		public void SingletonSameInterface()
		{
			Assert.AreSame(_root.Resolve<ISingletonService>(), _root.Resolve<ISingletonService>());
		}
		[Test]
		public void SingletonDifferentInterfaces()
		{
			Assert.AreSame(_root.Resolve<ISingletonService>(), _root.Resolve<IOtherSingletonService>());
		}
	}
}

