using System;
using Autofac;
using Autocore.Implementation;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Autocore.Test
{
	public class FactoryTest : TestBase
	{
		public interface IService : ISingletonDependency
		{
			string Text { get; }
		}
		public class MyService : IService
		{
			public const string TEXT = "Hi!";
			public string Text { get { return TEXT; } }
		}
		public interface IGenericService<T> : ISingletonDependency
		{
			string Value { get; }
		}
		public class MyGenericService<T> : IGenericService<T>
		{
			public string Value { get { return typeof(T).FullName; } }
		}

		[Test]
		public void Create()
		{
			using (var container = Factory.Create(asm => asm == typeof(FactoryTest).Assembly)) 
			{
				Assert.AreEqual(MyService.TEXT, container.Resolve<IService>().Text);
				Assert.AreEqual(typeof(int).FullName, container.Resolve<IGenericService<int>>().Value);
			}
		}
	}
}

