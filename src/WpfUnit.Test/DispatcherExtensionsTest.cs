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
		public void TestGetActiveDispatcherTimers1()
		{
			Dispatcher dispatcher = null;
			new Action(() => dispatcher.GetActiveDispatcherTimers())
				.Should().Throw<NullReferenceException>();
		}

		[Test]
		public void TestGetActiveDispatcherTimers2()
		{
			var dispatcher = Dispatcher.CurrentDispatcher;
			dispatcher.GetActiveDispatcherTimers().Should().BeEmpty("because we haven't started any dispatcher timer");
		}

		[Test]
		public void TestGetActiveDispatcherTimers3()
		{
			var dispatcher = Dispatcher.CurrentDispatcher;
			var timer = new DispatcherTimer();
			dispatcher.GetActiveDispatcherTimers().Should().BeEmpty("because we haven't started any dispatcher timer");
			GC.KeepAlive(timer);
		}

		[Test]
		public void TestGetActiveDispatcherTimers4()
		{
			var dispatcher = Dispatcher.CurrentDispatcher;
			var timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Input, (sender, args) => { }, dispatcher);
			timer.Start();
			dispatcher.GetActiveDispatcherTimers().Should().BeEquivalentTo(new object[] {timer}, "because we've just started that timer");
			timer.Stop();
			dispatcher.GetActiveDispatcherTimers().Should().BeEmpty("because we've just stopped the last timer");
		}

		[Test]
		public void TestGetActiveDispatcherTimers5()
		{
			var dispatcher = Dispatcher.CurrentDispatcher;
			var timer1 = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Input, (sender, args) => { }, dispatcher);
			timer1.Start();
			dispatcher.GetActiveDispatcherTimers().Should().BeEquivalentTo(new object[] { timer1 }, "because we've just started that timer");

			var timer2 = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Input, (sender, args) => { }, dispatcher);
			timer2.Start();
			dispatcher.GetActiveDispatcherTimers().Should().BeEquivalentTo(new object[] {timer1, timer2}, "because we've just started a 2nd timer");

			timer1.Stop();
			dispatcher.GetActiveDispatcherTimers().Should().BeEquivalentTo(new object[] {timer2}, "because only one timer is left");

			timer2.Stop();
			dispatcher.GetActiveDispatcherTimers().Should().BeEmpty("because we've just stopped the last timer");
		}

		[Test]
		public void TestExecutePendingEvents1()
		{
			Dispatcher dispatcher = null;
			new Action(() => dispatcher.ExecutePendingEvents())
				.Should().Throw<NullReferenceException>();
		}

		[Test]
		[Timeout(10000)]
		[Description("Verifies that ExecutePendingEvents doesn't do anything when no events are pending")]
		public void TestExecutePendingEvents2()
		{
			var dispatcher = Dispatcher.CurrentDispatcher;
			new Action(() => dispatcher.ExecutePendingEvents()).ExecutionTime().Should().BeLessOrEqualTo(TimeSpan.FromSeconds(1));
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