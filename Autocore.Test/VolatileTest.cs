using System;
using Autofac;
using Autocore.Implementation;
using Moq;
using NUnit.Framework;

namespace Autocore.Test
{
	public class VolatileTest : TestBase
	{
		public interface IVolatileObject : IVolatileDependency {}

		[Test]
		public void Resolve()
		{
			int calls = 0;
			var volatileObject = Create<IVolatileObject>().Object;
			var volatileContext = Create<IVolatileContext>();
			volatileContext.Setup(o => o.Resolve<IVolatileObject>()).Returns(volatileObject).Callback(() => ++calls);

			var v = new Volatile<IVolatileObject>(volatileContext.Object);

			Assert.AreEqual(0, calls);
			Assert.AreEqual(volatileObject, v.Value);
			Assert.AreEqual(1, calls);
			Assert.AreEqual(volatileObject, v.Value);
			Assert.AreEqual(2, calls);
		}
	}
}

