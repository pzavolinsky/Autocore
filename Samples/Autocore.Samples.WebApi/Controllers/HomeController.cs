using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Autocore.Samples.WebApi.Controllers
{
	public interface IRandomGreeting : IVolatileDependency
	{
		string Greeting { get; }
	}
	public class RandomGreeting : IRandomGreeting
	{
		static string[] GREETINGS = { "Hi", "Hello", "Whassup!", "Yo" };
		public RandomGreeting()
		{
			Greeting = GREETINGS[new Random().Next(GREETINGS.Length)];
		}
		public string Greeting { get; protected set; }
	}
	public interface IHelloMessage : ISingletonDependency
	{
		string Message { get; }
	}
	public class HelloMessage : IHelloMessage
	{
		Volatile<IRandomGreeting> _greeting;
		public HelloMessage(Volatile<IRandomGreeting> greeting)
		{
			_greeting = greeting;
		}
		public string Message
		{
			get
			{
				return string.Format(
					"{0}, Welcome to ASP.NET MVC injected with Autocore!\n" +
					"\n" +
					"My name is {1} (should always be same, since I'm a singleton)\n" +
					"\n" +
					"My greeter instance is {2} (show change with each request) and it always greets you with '{3}'",
					_greeting.Value.Greeting,
					GetHashCode(),
					_greeting.Value.GetHashCode(), _greeting.Value.Greeting);
			}
		}
	}

	public class HomeController : ApiController
	{
		IHelloMessage _hello;
		public HomeController(IHelloMessage hello)
		{
			_hello = hello;
		}
		public IHttpActionResult GetIndex()
		{
			return Ok(_hello.Message);
		}
	}
}
