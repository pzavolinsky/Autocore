using System;
using Moq;
using NUnit.Framework;
using System.Reflection;

namespace Autocore.Test
{
	public class IntegrationBase : TestBase
	{
		protected IContainer _root;

		protected override void SetupUpTest()
		{
			_root = Factory.Create(new Assembly[] { typeof(IntegrationBase).Assembly });
		}
		protected override void TearDownTest()
		{
			_root.Dispose();
		}
	}
}
