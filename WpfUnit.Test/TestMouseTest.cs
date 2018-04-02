using System.Threading;
using System.Windows;
using System.Windows.Input;
using FluentAssertions;
using NUnit.Framework;

namespace WpfUnit.Test
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class TestMouseTest
	{
		[Test]
		public void TestMove1()
		{
			var mouse = new TestMouse();
			var control = new TestControl();

			control.Position.Should().Be(new Point(0, 0));
			mouse.MoveRelativeTo(control, new Point(42, 24));
			control.Position.Should().Be(new Point(42, 24));
		}

		[Test]
		public void TestRotateMouseWheel()
		{
			var mouse = new TestMouse();
			var control = new TestControl();

			mouse.RotateMouseWheel(control, 1);
			control.WheelDelta.Should().Be(1);
			mouse.RotateMouseWheel(control, -1);
			control.WheelDelta.Should().Be(-1);
		}

		[Test]
		public void TestRotateMouseWheelUp()
		{
			var mouse = new TestMouse();
			var control = new TestControl();

			mouse.RotateMouseWheelUp(control);
			control.WheelDelta.Should().Be(120);
		}

		[Test]
		public void TestRotateMouseWheelDown()
		{
			var mouse = new TestMouse();
			var control = new TestControl();

			mouse.RotateMouseWheelDown(control);
			control.WheelDelta.Should().Be(-120);
		}
	}

	public sealed class TestControl
		: UIElement
	{
		public TestControl()
		{
			MouseMove += OnMouseMove;
			MouseWheel += OnMouseWheel;
		}

		private void OnMouseWheel(object sender, MouseWheelEventArgs args)
		{
			WheelDelta = args.Delta;
		}

		private void OnMouseMove(object sender, MouseEventArgs args)
		{
			Position = Mouse.GetPosition(this);
		}

		public int WheelDelta;
		public Point Position;
	}
}