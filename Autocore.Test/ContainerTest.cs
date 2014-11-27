using System;
using Autofac;
using Autocore.Implementation;
using Moq;
using NUnit.Framework;

namespace Autocore.Test
{
	public class ContainerTest : TestBase
	{
		public interface INonVolatileObject : INonVolatileDependency {}

		[Test]
		public void Dispose()
		{
			var scope = Create<ILifetimeScope>();
			scope.Setup(o => o.Dispose());
			using (var container = new Container(scope.Object)) 
			{
				// do nothing
			}
		}

		[Test]
		public void Resolve()
		{
			var nonVolatileObject = Create<INonVolatileObject>().Object;
			var componentRegistration = Create<Autofac.Core.IComponentRegistration>().Object;
			var componentRegistry = Create<Autofac.Core.IComponentRegistry>();
			componentRegistry.Setup(o => o.TryGetRegistration(It.IsAny<Autofac.Core.TypedService>(), out componentRegistration)).Returns(true);
			var scope = Create<ILifetimeScope>();
			scope.Setup(o => o.ComponentRegistry).Returns(componentRegistry.Object);
			scope.Setup(o => o.ResolveComponent(componentRegistration, It.IsAny<Autofac.Core.Parameter[]>())).Returns(nonVolatileObject);

			var container = new Container(scope.Object);

			Assert.AreEqual(nonVolatileObject, container.Resolve<INonVolatileObject>());
		}

		[Test]
		public void CreateScope()
		{
			var childScope = Create<ILifetimeScope>().Object;
			var scope = Create<ILifetimeScope>();
			scope.Setup(o => o.BeginLifetimeScope()).Returns(childScope);

			var container = new Container(scope.Object);
			var childContainer = container.CreateScope();

			Assert.AreEqual(childScope, ((Container) childContainer).Scope);
		}

		[Test]
		public void CreateVolatileScope()
		{
			var childScope = Create<ILifetimeScope>().Object;
			var scope = Create<ILifetimeScope>();
			scope.Setup(o => o.BeginLifetimeScope(Container.VOLATILE_TAG)).Returns(childScope);

			var container = new Container(scope.Object);
			var childContainer = container.CreateVolatileScope();

			Assert.AreEqual(childScope, ((VolatileContainer) childContainer).Scope);
		}
	}
}

