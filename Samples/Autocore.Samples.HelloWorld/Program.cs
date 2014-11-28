using System;

namespace Autocore.Samples.HelloWorld
{
	public interface IGreeting : ISingletonDependency
	{
		string Message { get; }
	}

	public class Greeting : IGreeting
	{
		public string Message
		{
			get { return "Hello World!"; }
		}
	}

	public static class Program
	{
		public static void Main(string[] args)
		{
			using (var container = Autocore.Factory.Create())
			{
				var greeting = container.Resolve<IGreeting>();
				System.Console.WriteLine(greeting.Message);
			}
		}
	}
}
