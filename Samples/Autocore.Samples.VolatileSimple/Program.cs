using System;
using System.Threading;
using System.Collections.Generic;

namespace Autocore.Samples.VolatileSimple
{
	#region Operation start time service
	public interface IOperationStartTime : IVolatileDependency
	{
		DateTime StartTime { get; }
	}
	public class OperationStartTime : IOperationStartTime
	{
		public OperationStartTime()
		{
			StartTime = DateTime.Now;
		}
		public DateTime StartTime { get; protected set; }
	}
	#endregion

	#region Operation service
	public interface IOperation : ISingletonDependency
	{
		void Work();
	}
	public class PrintTimeOperation : IOperation
	{
		Volatile<IOperationStartTime> _time;
		public PrintTimeOperation(Volatile<IOperationStartTime> time)
		{
			_time = time;
		}
		public void Work()
		{
			Console.WriteLine("Operation start time: {0}", _time.Value.StartTime.Ticks);
			Thread.Sleep(50);
			Console.WriteLine("Again, just in case:  {0}", _time.Value.StartTime.Ticks);
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
				// Note that multiple calls to Resolve<IOperation> yield
				// the same instance.
				var op = container.Resolve<IOperation>();

				// First call
				Console.WriteLine("Calling Work for the first time:");
				container.ExecuteInVolatileScope(scope => {
					op.Work();
				});

				Thread.Sleep(50);
				Console.WriteLine();

				// Second call
				Console.WriteLine("Calling Work a second time:");
				container.ExecuteInVolatileScope(scope => {
					op.Work();
				});

				Console.WriteLine();

				// Call outside a volatile scope
				try
				{
					Console.WriteLine("Calling Work outside a volatile scope");
					op.Work();
					Console.WriteLine("Oops something went wrong, this line should not have executed");
				}
				catch (InvalidOperationException e)
				{
					Console.WriteLine("Failed as expected: {0}", e.Message);
				}

			}
		}
	}
}
