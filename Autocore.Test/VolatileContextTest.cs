using System;
using Autocore;
using Moq;
using NUnit.Framework;
using Autocore.Interfaces;
using Autocore.Implementation;
using System.Collections.Generic;

namespace Autocore.Test
{
	public class VolatileContextTest : TestBase
	{
		public interface IVolatileObject : IVolatileDependency {}

		[Test]
		public void Resolve()
		{
			var implicitContext = Create<IImplicitContext>();
			var volatileContext = new VolatileContext(implicitContext.Object);
			var container = Create<IVolatileContainer>();
			var volatileObject = Create<IVolatileObject>().Object;

			container.Setup(o => o.Resolve<IVolatileObject>()).Returns(volatileObject);
			implicitContext.SetupGet(o => o.Container).Returns(container.Object);

			Assert.AreEqual(volatileObject, volatileContext.Resolve<IVolatileObject>());
		}

		[Test]
		public void ResolveEnumerable()
		{
			var implicitContext = Create<IImplicitContext>();
			var volatileContext = new VolatileContext(implicitContext.Object);
			var container = Create<IVolatileContainer>();
			var volatileObject = Create<List<IVolatileObject>>().Object;

			container.Setup(o => o.ResolveEnumerable<IVolatileObject>()).Returns(volatileObject);
			implicitContext.SetupGet(o => o.Container).Returns(container.Object);

			Assert.AreEqual(volatileObject, volatileContext.ResolveEnumerable<IVolatileObject>());
		}

		[Test]
		public void ResolveOutsideOfVolatileScope()
		{
			var implicitContext = Create<IImplicitContext>();
			var volatileContext = new VolatileContext(implicitContext.Object);
			implicitContext.SetupGet(o => o.Container).Returns((IVolatileContainer) null);

			Assert.Throws<VolatileResolutionException>(() => volatileContext.Resolve<IVolatileObject>());
		}

		[Test]
		public void ResolveEnumerableOutsideOfVolatileScope()
		{
			var implicitContext = Create<IImplicitContext>();
			var volatileContext = new VolatileContext(implicitContext.Object);
			implicitContext.SetupGet(o => o.Container).Returns((IVolatileContainer) null);

			Assert.Throws<VolatileResolutionException>(() => volatileContext.ResolveEnumerable<IVolatileObject>());
		}
	}
}
