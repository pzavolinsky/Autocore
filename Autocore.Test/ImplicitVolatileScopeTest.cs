using System;
using Autofac;
using Autocore.Implementation;
using Moq;
using NUnit.Framework;

namespace Autocore.Test
{
	public class ImplicitVolatileScopeTest : TestBase
	{
		[Test]
		public void Dispose()
		{
			var implicitContext = Create<IImplicitContext>();
			implicitContext.SetupProperty(o => o.Container);
			var newContainer = Create<IVolatileContainer>();
			newContainer.Setup(o => o.Dispose());
			var rootContainer = Create<IContainer>();
			rootContainer.Setup(o => o.Resolve<IImplicitContext>()).Returns(implicitContext.Object);
			rootContainer.Setup(o => o.CreateVolatileScope()).Returns(newContainer.Object);

			Assert.AreEqual(null, implicitContext.Object.Container);
			using (var implicitVolatileScope = new ImplicitVolatileScope(rootContainer.Object)) 
			{
				Assert.AreEqual(newContainer.Object, implicitContext.Object.Container);
			}
			Assert.AreEqual(null, implicitContext.Object.Container);
		}
	}
}

