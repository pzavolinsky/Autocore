using System;
using Moq;
using NUnit.Framework;

namespace Autocore.Test.Integration
{
	public class InstanceTest : IntegrationBase
	{
		public interface IInstanceService : IInstanceDependency {}
		public class MyInstanceService : IInstanceService {}
		
		[Test]
		public void Instance()
		{
			Assert.AreNotSame(_root.Resolve<IInstanceService>(), _root.Resolve<IInstanceService>());
		}
	}
}

