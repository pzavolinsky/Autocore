using System;
using Autofac;
using Autocore.Implementation;
using Moq;
using NUnit.Framework;

namespace Autocore.Test
{
	public class VolatileContainerTest : TestBase
	{
		public interface IVolatileObject : IVolatileDependency {}

		[Test]
		public void Resolve()
		{
			var volatileObject = Create<IVolatileObject>().Object;
			var componentRegistration = Create<Autofac.Core.IComponentRegistration>().Object;
			var componentRegistry = Create<Autofac.Core.IComponentRegistry>();
			componentRegistry.Setup(o => o.TryGetRegistration(It.IsAny<Autofac.Core.TypedService>(), out componentRegistration)).Returns(true);
			var scope = Create<ILifetimeScope>();
			scope.Setup(o => o.ComponentRegistry).Returns(componentRegistry.Object);
			scope.Setup(o => o.ResolveComponent(componentRegistration, It.IsAny<Autofac.Core.Parameter[]>())).Returns(volatileObject);

			var container = new VolatileContainer(scope.Object);

			Assert.AreEqual(volatileObject, container.Resolve<IVolatileObject>());
		}
	}
}

