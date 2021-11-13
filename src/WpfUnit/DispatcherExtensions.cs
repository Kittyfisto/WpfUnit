using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;

namespace WpfUnit
{
	/// <summary>
	///     Extension methods to the <see cref="Dispatcher" /> class.
	/// </summary>
	public static class DispatcherExtensions
	{
		/// <summary>
		///     Returns the list of currently active (started and not stopped since) dispatcher timers.
		/// </summary>
		/// <param name="dispatcher"></param>
		/// <returns></returns>
		[Pure]
		public static IEnumerable<DispatcherTimer> GetActiveDispatcherTimers(this Dispatcher dispatcher)
		{
			if (dispatcher == null)
				throw new NullReferenceException();

			var field = typeof(Dispatcher).GetField("_timers", BindingFlags.Instance | BindingFlags.NonPublic);
			var value = field.GetValue(dispatcher);

			// This method is intended to return a snapshot of the list of timers, hence the clone
			return ((IEnumerable<DispatcherTimer>) value)?.ToList();
		}

		/// <summary>
		///     Blocks until all pending events on this dispatcher have been executed.
		///     Pending events are those events that have been added through
		///     <see cref="Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority,System.Delegate)" />
		///     and its overloadeds.
		/// </summary>
		/// <param name="dispatcher"></param>
		public static void ExecutePendingEvents(this Dispatcher dispatcher)
		{
			if (dispatcher == null)
				throw new NullReferenceException();

			var frame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
				new DispatcherOperationCallback(ExitFrame), frame);
			Dispatcher.PushFrame(frame);
		}

		private static object ExitFrame(object frame)
		{
			((DispatcherFrame) frame).Continue = false;
			return null;
		}
	}
}