using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Autocore.Samples.VolatileAsync
{
	#region User service
	public interface IUser : IVolatileDependency
	{
		string Name { get; set; }
	}
	public class User : IUser
	{
		public string Name { get; set; }
	}
	#endregion

	#region Operation service
	public interface IUserGreeter : ISingletonDependency
	{
		Task GreetAsync(string shouldBe);
	}
	public class UserGreeter : IUserGreeter
	{
		IVolatile<IUser> _user;
		public UserGreeter(IVolatile<IUser> user)
		{
			_user = user;
		}
		public async Task GreetAsync(string shouldBe)
		{
			if (_user.Value.Name == shouldBe)
			{
				await Task.Delay(50);
				if (_user.Value.Name == shouldBe) // still valid
				{
					Console.WriteLine("Hello: {0}!", _user.Value.Name);
				} else {
					Console.WriteLine("Ooops: {0}, you used to be '{1}' but you are not anymore", _user.Value.Name, shouldBe);
				}
			} else {
				Console.WriteLine("Ooops: {0}, you should be '{1}'", _user.Value.Name, shouldBe);
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
				// Resolve once, then cache for multiple operation calls.
				// Note that multiple calls to Resolve<IUserGreeter> yield
				// the same instance.
				var op = container.Resolve<IUserGreeter>();

				var tasks = new List<Task>();
				var rnd = new Random();

				// Create 10 tasks each with a different user name.
				for (int i = 0; i < 10; ++i)
				{
					tasks.Add(container.ExecuteInVolatileScopeAsync(async scope => {
						string name = string.Format("User {0}", i);
						scope.Resolve<IUser>().Name = name;
						await Task.Delay(rnd.Next(200));
						await op.GreetAsync(name);
					}));
				}

				// Start the tasks in random order.
				var randomTasks = tasks.OrderBy(t => rnd.Next()).ToArray();

				// Wait until all the tasks are done.
				Task.WaitAll(randomTasks);
			}
		}
	}
}
