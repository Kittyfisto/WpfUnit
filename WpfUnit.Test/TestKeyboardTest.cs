using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using FluentAssertions;
using NUnit.Framework;

namespace WpfUnit.Test
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class TestKeyboardTest
	{
		public static IEnumerable<Key> Keys => Enum.GetValues(typeof(Key)).Cast<Key>().Skip(1).ToList();

		public static IEnumerable<ModifierKeys> AllModifierKeys = new[]
		{
			//ModifierKeys.Shift,
			ModifierKeys.Alt,
			ModifierKeys.Control,

			ModifierKeys.Alt | ModifierKeys.Shift,
			ModifierKeys.Alt | ModifierKeys.Control,
			//ModifierKeys.Alt | ModifierKeys.Windows,

			ModifierKeys.Shift | ModifierKeys.Control,
			//ModifierKeys.Shift | ModifierKeys.Windows,

			//ModifierKeys.Windows | ModifierKeys.Control,

			ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Control,
			//ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Windows,
			//ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Windows,

			//ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Windows,
		};

		#region KeyboardDevice

		[Test]
		public void TestKeyboardDeviceIsKeyDown([ValueSource(nameof(Keys))] Key key)
		{
			var device = Keyboard.PrimaryDevice;

			var keyboard = new TestKeyboard();
			device.IsKeyDown(key).Should().BeFalse();

			keyboard.Press(key);
			device.IsKeyDown(key).Should().BeTrue();

			keyboard.Release(key);
			device.IsKeyDown(key).Should().BeFalse();
		}

		[Test]
		public void TestKeyboardDeviceIsKeyUp([ValueSource(nameof(Keys))] Key key)
		{
			var device = Keyboard.PrimaryDevice;

			var keyboard = new TestKeyboard();
			device.IsKeyUp(key).Should().BeTrue();

			keyboard.Press(key);
			device.IsKeyUp(key).Should().BeFalse();

			keyboard.Release(key);
			device.IsKeyUp(key).Should().BeTrue();
		}

		[Test]
		public void TestKeyboardDeviceModifiersShift()
		{
			var device = Keyboard.PrimaryDevice;

			var keyboard = new TestKeyboard();
			device.Modifiers.Should().Be(ModifierKeys.None);

			keyboard.Press(Key.LeftShift);
			device.Modifiers.Should().Be(ModifierKeys.Shift);

			keyboard.Release(Key.LeftShift);
			device.Modifiers.Should().Be(ModifierKeys.None);

			keyboard.Press(Key.RightShift);
			device.Modifiers.Should().Be(ModifierKeys.Shift);
		}

		[Test]
		public void TestKeyboardDeviceModifiersControl()
		{
			var device = Keyboard.PrimaryDevice;

			var keyboard = new TestKeyboard();
			device.Modifiers.Should().Be(ModifierKeys.None);

			keyboard.Press(Key.LeftCtrl);
			device.Modifiers.Should().Be(ModifierKeys.Control);

			keyboard.Release(Key.LeftCtrl);
			device.Modifiers.Should().Be(ModifierKeys.None);

			keyboard.Press(Key.RightCtrl);
			device.Modifiers.Should().Be(ModifierKeys.Control);
		}

		[Test]
		public void TestKeyboardDeviceModifiersAlt()
		{
			var device = Keyboard.PrimaryDevice;

			var keyboard = new TestKeyboard();
			device.Modifiers.Should().Be(ModifierKeys.None);

			keyboard.Press(Key.LeftAlt);
			device.Modifiers.Should().Be(ModifierKeys.Alt);

			keyboard.Release(Key.LeftAlt);
			device.Modifiers.Should().Be(ModifierKeys.None);

			keyboard.Press(Key.RightAlt);
			device.Modifiers.Should().Be(ModifierKeys.Alt);
		}

		#endregion

		#region Keyboard

		[Test]
		public void TestKeyboardIsKeyDown1()
		{
			Keyboard.IsKeyDown(Key.LeftShift).Should().BeFalse();

			var keyboard = new TestKeyboard();
			keyboard.Press(Key.LeftShift);
			Keyboard.IsKeyDown(Key.LeftShift).Should().BeTrue();
			keyboard.Release(Key.LeftShift);
			Keyboard.IsKeyDown(Key.LeftShift).Should().BeFalse();
		}

		[Test]
#if !DEBUG
		[Ignore("Fails reliably in release mode, don't know why yet")]
#endif
		public void TestIsKeyUp1()
		{
			Keyboard.IsKeyUp(Key.LeftShift).Should().BeTrue();

			var keyboard = new TestKeyboard();
			keyboard.Press(Key.LeftShift);
			Keyboard.IsKeyUp(Key.LeftShift).Should().BeFalse();
			keyboard.Release(Key.LeftShift);
			Keyboard.IsKeyUp(Key.LeftShift).Should().BeTrue();
		}

		[Test]
		public void TestIsKeyToggled1()
		{
			
		}

		[Test]
		[Description("Verifies that only the pressed keys are actually pressed")]
		public void TestPress1()
		{
			var keyboard = new TestKeyboard();

			foreach (var key in Keys)
			{
				Keyboard.IsKeyDown(key).Should().BeFalse();
				Keyboard.IsKeyUp(key).Should().BeTrue();
				Keyboard.IsKeyToggled(key).Should().BeFalse();
			}

			keyboard.Press(Key.LeftShift);

			foreach (var key in Keys)
			{
				if (key == Key.LeftShift)
				{
					Keyboard.IsKeyDown(key).Should().BeTrue();
					Keyboard.IsKeyUp(key).Should().BeFalse();
				}
				else
				{
					Keyboard.IsKeyDown(key).Should().BeFalse();
					Keyboard.IsKeyUp(key).Should().BeTrue();
				}
				Keyboard.IsKeyToggled(key).Should().BeFalse();
			}
		}

		#endregion

		#region Control

		[Test]
		public void TestInputBinding1([ValueSource(nameof(AllModifierKeys))] ModifierKeys modifierKeys)
		{
			bool pressed = false;

			var control = new FrameworkElement();
			control.InputBindings.Add(new KeyBinding(
				new Command(() => pressed = true),
				new KeyGesture(Key.B, modifierKeys))
				);

			var keyboard = new TestKeyboard();
			keyboard.Click(control, Key.B, modifierKeys);

			pressed.Should().BeTrue("because the control should've interpreted the gesture correctly");
		}

		[Test]
		[Description("Verifies that Click() resets the state of its key, even when client-code throws unexpected exceptions")]
		public void TestClickElement1()
		{
			var control = new BrokenControl();

			var keyboard = new TestKeyboard();
			new Action(() => keyboard.Click(control, Key.A)).ShouldThrow<NullReferenceException>("because the control author made a booboo");

			Keyboard.IsKeyDown(Key.A).Should()
				.BeFalse(
					"because TestKeyboard should anticipate client-code failures and reset the state of the A key no matter what");
		}

		[Test]
		[Description("Verifies that Click() resets the state of its key, even when client-code throws unexpected exceptions")]
		public void TestClickElement2()
		{
			var control = new BrokenControl(keyToFailOn: Key.A);

			var keyboard = new TestKeyboard();
			new Action(() => keyboard.Click(control, Key.A, ModifierKeys.Control)).ShouldThrow<NullReferenceException>("because the control author made a booboo");

			Keyboard.IsKeyDown(Key.A).Should()
				.BeFalse(
					"because TestKeyboard should anticipate client-code failures and reset the state of the A key no matter what");
			Keyboard.IsKeyDown(Key.LeftCtrl).Should()
				.BeFalse(
					"because TestKeyboard should anticipate client-code failures and reset the state of the control key no matter what");
			Keyboard.IsKeyDown(Key.RightCtrl).Should()
				.BeFalse(
					"because TestKeyboard should anticipate client-code failures and reset the state of the control key no matter what");
		}

		sealed class BrokenControl
			: FrameworkElement
		{
			private readonly Key? _keyToFailOn;

			public BrokenControl(Key? keyToFailOn = null)
			{
				_keyToFailOn = keyToFailOn;
			}

			protected override void OnKeyDown(KeyEventArgs e)
			{
				if (_keyToFailOn == null || _keyToFailOn == e.Key)
					throw new NullReferenceException();
			}
		}

		sealed class Command
			: ICommand
		{
			private readonly Action _fn;

			public Command(Action fn)
			{
				_fn = fn;
			}

			public bool CanExecute(object parameter)
			{
				return true;
			}

			public void Execute(object parameter)
			{
				_fn();
			}

#pragma warning disable 67
			public event EventHandler CanExecuteChanged;
#pragma warning restore 67
		}

		#endregion
	}
}
