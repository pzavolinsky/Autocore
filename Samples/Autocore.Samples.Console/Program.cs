using System;
using System.Collections.Generic;

namespace Autocore.Samples.Console
{
	#region Output services
	public interface IOutput : ISingletonDependency
	{
		void WriteLine(string text);
	}
	public class ConsoleOutput : IOutput
	{
		public virtual void WriteLine(string text)
		{
			System.Console.WriteLine(string.Format("ConsoleOutput: {0}", text));
		}
	}
	public class ColorConsoleOutput : IOutput
	{
		public virtual void WriteLine(string text)
		{
			System.Console.ForegroundColor = ConsoleColor.Yellow;
			System.Console.Write("ColorConsoleOutput: ");
			System.Console.ForegroundColor = ConsoleColor.Gray;
			System.Console.WriteLine(text);
		}
	}
	#endregion

	#region Greeter service
	public interface IGreeter : ISingletonDependency
	{
		void Greet(string name);
	}
	public class Greeter : IGreeter
	{
		IEnumerable<IOutput> _outputs;
		public Greeter(IEnumerable<IOutput> outputs)
		{
			_outputs = outputs;
		}
		public void Greet(string name)
		{
			var msg = string.Format("Hello {0}!", name);
			foreach (var o in _outputs)
			{
				o.WriteLine(msg);
			}
		}
	}
	#endregion

	public static class Program
	{
		public static void Main(string[] args)
		{
			using (var container = Autocore.Factory.Create())
			{
				var greeter = container.Resolve<IGreeter>();
				greeter.Greet("John");
			}
		}
	}
}
