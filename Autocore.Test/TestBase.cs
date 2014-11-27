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
		}

		[TearDown]
		public void TearDown()
		{
			_repo.VerifyAll();
		}

		public Mock<T> Create<T>() where T : class
		{
			return _repo.Create<T>();
		}
	}
}

