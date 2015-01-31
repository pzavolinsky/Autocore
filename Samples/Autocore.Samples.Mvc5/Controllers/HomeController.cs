using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace Autocore.Samples.Mvc5.Controllers
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
		IVolatile<IRandomGreeting> _greeting;
		public HelloMessage(IVolatile<IRandomGreeting> greeting)
		{
			_greeting = greeting;
		}
		public string Message
		{
			get {
				return string.Format(
					"{0}, Welcome to ASP.NET MVC injected with Autocore!\n"+
					"\n"+
					"My name is {1} (should always be same, since I'm a singleton)\n"+
					"\n"+
					"My greeter instance is {2} (show change with each request) and it always greets you with '{3}'",
					_greeting.Value.Greeting,
					GetHashCode(),
					_greeting.Value.GetHashCode(), _greeting.Value.Greeting);
			}
		}
	}

	public class HomeController : Controller
	{
		IHelloMessage _hello;
		public HomeController(IHelloMessage hello)
		{
			_hello = hello;
		}
		public ActionResult Index()
		{
			ViewBag.Message = _hello.Message;
			return View();
		}
	}
}

