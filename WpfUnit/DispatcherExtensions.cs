using System;
using System.Windows.Threading;

namespace WpfUnit
{
	/// <summary>
	///     Extension methods to the <see cref="Dispatcher" /> class.
	/// </summary>
	public static class DispatcherExtensions
	{
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