using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Autocore.Samples.VolatileSync
{
	#region Operation start time service
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
		void Greet(string shouldBe);
	}
	public class UserGreeter : IUserGreeter
	{
		Volatile<IUser> _user;
		public UserGreeter(Volatile<IUser> user)
		{
			_user = user;
		}
		public void Greet(string shouldBe)
		{
			if (_user.Value.Name == shouldBe)
		    {
				Console.WriteLine("Hello: {0}!", _user.Value.Name);
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
					tasks.Add(new Task(id => {
						container.ExecuteInVolatileScope(scope => {
							string name = string.Format("User {0}", id);
							scope.Resolve<IUser>().Name = name;
							Thread.Sleep(rnd.Next(200));
							op.Greet(name);
						});
					}, i));
				}

				// Start the tasks in random order.
				var randomTasks = tasks.OrderBy(t => rnd.Next()).ToArray();
				foreach (var task in randomTasks) 
				{
					task.Start();
				}

				// Wait until all the tasks are done.
				Task.WaitAll(randomTasks);
			}
		}
	}
}
