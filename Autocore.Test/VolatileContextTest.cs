using System;
using Autocore;
using Moq;
using NUnit.Framework;
using Autocore.Implementation;

namespace Autocore.Test
{
	public class VolatileContextTest : TestBase
	{
		public interface IVolatileObject : IVolatileDependency {}

		[Test]
		public void ResolveTest()
		{
			var implicitContext = Create<IImplicitContext>();
			var volatileContext = new VolatileContext(implicitContext.Object);
			var container = Create<IVolatileContainer>();
			var volatileObject = Create<IVolatileObject>().Object;

			container.Setup(o => o.Resolve<IVolatileObject>()).Returns(volatileObject);
			implicitContext.SetupGet(o => o.Container).Returns(container.Object);

			Assert.AreEqual(volatileObject, volatileContext.Resolve<IVolatileObject>());
		}
	}
}

