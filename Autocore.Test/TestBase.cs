using System;
using Autocore;
using Moq;
using NUnit.Framework;

namespace Autocore.Test
{
	[TestFixture]
	public class TestBase
	{
		private MockRepository _repo;

		[SetUp]
		public void SetUp()
		{
			_repo = new MockRepository(MockBehavior.Strict);
			SetupUpTest();
		}

		[TearDown]
		public void TearDown()
		{
			TearDownTest();
			_repo.VerifyAll();
		}

		public Mock<T> Create<T>() where T : class
		{
			return _repo.Create<T>();
		}

		protected virtual void SetupUpTest() {}
		protected virtual void TearDownTest() {}
	}
}

