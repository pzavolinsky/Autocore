using System;
using Moq;
using NUnit.Framework;

namespace Autocore.Test.Integration
{
	public class GenericSingletonTest : IntegrationBase
	{
		public interface IGenericService<T> : ISingletonDependency { string Value { get; } }
		public class MyGenericService<T> : IGenericService<T>
		{
			public string Value { get { return typeof(T).FullName; } }
		}

		[Test]
		public void GenericSingletonSameT()
		{
			var svc1 = _root.Resolve<IGenericService<int>>();
			var svc2 = _root.Resolve<IGenericService<int>>();
			Assert.AreSame(svc1, svc2);
			Assert.AreEqual(typeof(int).FullName, svc1.Value);
		}

		[Test]
		public void GenericSingletonDifferentT()
		{
			var svc1 = _root.Resolve<IGenericService<int>>();
			var svc2 = _root.Resolve<IGenericService<string>>();
			Assert.AreNotSame(svc1, svc2);
			Assert.AreEqual(typeof(int).FullName, svc1.Value);
			Assert.AreEqual(typeof(string).FullName, svc2.Value);
		}
	}
}

