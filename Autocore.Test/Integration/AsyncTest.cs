using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Autocore.Test.Integration
{
	public class AsyncTest : IntegrationBase
	{
		public interface IText : IVolatileDependency { string Text { get; set; } }
		public class User : IText { public string Text { get; set; } }

		public interface ITextChecker : ISingletonDependency 
		{
			Task<bool> CheckAsync(string expected);
			Task AssertAsync(string expected);
		}
		public class TextChecker : ITextChecker
		{
			IVolatile<IText> _text;
			public TextChecker(IVolatile<IText> text)
			{
				_text = text;
			}
			public async Task<bool> CheckAsync(string expected)
			{
				if (_text.Value.Text != expected) {
					return false;
				}
				await Task.Delay(50);
				return _text.Value.Text == expected; // still valid
			}
			public async Task AssertAsync(string expected)
			{
				if (_text.Value.Text == expected) {
					await Task.Delay(50);
					if (_text.Value.Text == expected) { // still valid
						return;
					}
				}
				throw new ArgumentException(string.Format("Assertion failed: {0} is not equal to {1}", _text.Value.Text, expected));
			}
		}

		[Test]
		public void AsyncVolatilesFunc()
		{
			var op = _root.Resolve<ITextChecker>();
			var tasks = new List<Task<bool>>();
			var rnd = new Random();
			for (int i = 0; i < 10; ++i)
			{
				tasks.Add(_root.ExecuteInVolatileScopeAsync(async scope => {
					string text = string.Format("{0}", i);
					scope.Resolve<IText>().Text = text;
					await Task.Delay(rnd.Next(20));
					return await op.CheckAsync(text);
				}));
			}

			Task.WaitAll(tasks.OrderBy(t => rnd.Next()).ToArray());

			foreach (var task in tasks) {
				Assert.IsTrue(task.Result);
			}
		}
		[Test]
		public void AsyncVolatilesAction()
		{
			var op = _root.Resolve<ITextChecker>();
			var tasks = new List<Task>();
			var rnd = new Random();
			for (int i = 0; i < 10; ++i)
			{
				tasks.Add(_root.ExecuteInVolatileScopeAsync(async scope => {
					string text = string.Format("{0}", i);
					scope.Resolve<IText>().Text = text;
					await Task.Delay(rnd.Next(20));
					await op.AssertAsync(text);
				}));
			}

			Task.WaitAll(tasks.OrderBy(t => rnd.Next()).ToArray());
		}
	}
}

