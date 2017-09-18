using System;
using System.Threading;
using System.Windows.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace WpfUnit.Test
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class DispatcherExtensionsTest
	{
		[Test]
		public void TestExecutePendingEvents1()
		{
			Dispatcher dispatcher = null;
			new Action(() => dispatcher.ExecutePendingEvents())
				.ShouldThrow<NullReferenceException>();
		}

		[Test]
		[Timeout(10000)]
		[Description("Verifies that ExecutePendingEvents doesn't do anything when no events are pending")]
		public void TestExecutePendingEvents2()
		{
			var dispatcher = Dispatcher.CurrentDispatcher;
			new Action(() => dispatcher.ExecutePendingEvents()).ExecutionTime().ShouldNotExceed(TimeSpan.FromSeconds(1));
		}

		[Test]
		public void TestExecutePendingEvents3()
		{
			var dispatcher = Dispatcher.CurrentDispatcher;
			bool called = false;
			dispatcher.BeginInvoke(new Action(() => called = true));
			called.Should().BeFalse("because the action may not have been invoked yet");

			dispatcher.ExecutePendingEvents();
			called.Should().BeTrue("because ExecutePendingEvents should've executed all pending actions");
		}

		[Test]
		[Description("Verifies that ExecutePendingEvents executes multiple events")]
		public void TestExecutePendingEvents4()
		{
			var dispatcher = Dispatcher.CurrentDispatcher;
			bool called1 = false;
			dispatcher.BeginInvoke(new Action(() => called1 = true));
			bool called2 = false;
			dispatcher.BeginInvoke(new Action(() => called2 = true));
			called1.Should().BeFalse("because the action may not have been invoked yet");
			called2.Should().BeFalse("because the action may not have been invoked yet");

			dispatcher.ExecutePendingEvents();
			called1.Should().BeTrue("because ExecutePendingEvents should've executed all pending actions");
			called2.Should().BeTrue("because ExecutePendingEvents should've executed all pending actions");
		}

		[Test]
		[Description("Verifies that ExecutePendingEvents even executes those events, that were added while executing other pending events")]
		public void TestExecutePendingEvents5()
		{
			var dispatcher = Dispatcher.CurrentDispatcher;
			bool called = false;
			dispatcher.BeginInvoke(new Action(() => dispatcher.BeginInvoke(new Action(() => called = true))));
			called.Should().BeFalse("because the action may not have been invoked yet");

			dispatcher.ExecutePendingEvents();
			called.Should().BeTrue("because ExecutePendingEvents should've executed ALL pending actions");
		}
	}
}