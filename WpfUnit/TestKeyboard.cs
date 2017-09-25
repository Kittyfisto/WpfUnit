using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Harmony;

namespace WpfUnit
{
	/// <summary>
	///     This class can be used to control the <see cref="Keyboard" /> class in a test scenario.
	///     By default, all keys are not pressed until <see cref="Press(Key)" /> is called.
	/// </summary>
	/// <remarks>
	///     Given the singleton nature of the <see cref="Keyboard" /> class, you should NOT enable
	///     parallel tests for your controls when using this class.
	/// </remarks>
	/// <example>
	///     Each test method should create its own <see cref="TestKeyboard" /> instance in order
	///     to not be influenced by previous test methods (that may have left keys in a pressed state).
	/// </example>
	public sealed class TestKeyboard
	{
		private static readonly HashSet<Key> PressedKeys;
		private readonly HwndSource _dummyInputSource;

		static TestKeyboard()
		{
			PressedKeys = new HashSet<Key>();

			AssemblySetup.EnsureIsPatched();
		}

		/// <summary>
		///     Initializes a new TestKeyboard.
		///     Defaults all keys to not being pressed.
		/// </summary>
		public TestKeyboard()
		{
			_dummyInputSource = new HwndSource(classStyle: 0, style: 0, exStyle: 0, x: 0, y: 0, name: "", parent: IntPtr.Zero);
			Reset();
		}

		/// <summary>
		///     Defaults all keys to not being pressed.
		/// </summary>
		public void Reset()
		{
			PressedKeys.Clear();
		}

		public void Press(Key key)
		{
			PressedKeys.Add(key);
		}

		public void Release(Key key)
		{
			PressedKeys.Remove(key);
		}

		/// <summary>
		///     Presses the given key and then notifies the given element of the key press.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="key"></param>
		public void Press(UIElement element, Key key)
		{
			Press(key);
			element.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,
				_dummyInputSource,
				Environment.TickCount,
				key)
			{
				RoutedEvent = UIElement.KeyDownEvent
			});
		}

		/// <summary>
		///     Releases the given key and then notifies the given element of the release.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="key"></param>
		public void Release(UIElement element, Key key)
		{
			Release(key);
			element.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,
				_dummyInputSource,
				Environment.TickCount,
				key)
			{
				RoutedEvent = UIElement.KeyUpEvent
			});
		}

		/// <summary>
		///     Presses and releases the given key, while notifying the given element.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="key"></param>
		public void Click(UIElement element, Key key)
		{
			try
			{
				Press(element, key);
			}
			finally
			{
				Release(element, key);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="element"></param>
		/// <param name="key"></param>
		/// <param name="modifierKeys"></param>
		public void Click(UIElement element, Key key, ModifierKeys modifierKeys)
		{
			var keys = new List<Key>();
			AddKeys(modifierKeys, keys);
			keys.Add(key);

			try
			{
				foreach (var k in keys)
					Press(element, k);
				foreach (var k in keys)
					Release(element, k);
			}
			catch(Exception)
			{
				// If we're here then that means that the custom control threw an unexpected exception.
				// There's no point in trying to notify the control of any further key releases
				// (because the Click operation has failed and shall rethrow that exception).
				// Therefore we only try to reset the state of the keyboard to what it was when Click()
				// was called and then call it a day...
				foreach (var k in keys)
					Release(k);

				throw;
			}
		}

		private static void AddKeys(ModifierKeys modifierKeys, List<Key> ret)
		{
			if (modifierKeys.HasFlag(ModifierKeys.Control))
				ret.Add(Key.LeftCtrl);
			if (modifierKeys.HasFlag(ModifierKeys.Alt))
				ret.Add(Key.LeftAlt);
			if (modifierKeys.HasFlag(ModifierKeys.Shift))
				ret.Add(Key.RightShift);
			if (modifierKeys.HasFlag(ModifierKeys.Windows))
				ret.Add(Key.LWin);
		}

		#region Keyboard

		[HarmonyPatch(typeof(Keyboard))]
		[HarmonyPatch("IsKeyDown")]
		private class PatchIsKeyDown
		{
			private static void Postfix(Key key, ref bool __result)
			{
				__result = PressedKeys.Contains(key);
			}
		}

		[HarmonyPatch(typeof(Keyboard))]
		[HarmonyPatch("IsKeyUp")]
		private class PatchIsKeyUp
		{
			private static void Postfix(Key key, ref bool __result)
			{
				__result = !PressedKeys.Contains(key);
			}
		}

		[HarmonyPatch(typeof(Keyboard))]
		[HarmonyPatch("IsKeyToggled")]
		private class PatchIsKeyToggled
		{
			private static void Postfix(Key key, ref bool __result)
			{
				__result = false;
			}
		}

		#endregion

		#region KeyboardDevice

		[HarmonyPatch(typeof(KeyboardDevice))]
		[HarmonyPatch("IsKeyDown")]
		private class PatchKeyboardDeviceIsKeyDown
		{
			private static void Postfix(Key key, ref bool __result)
			{
				__result = PressedKeys.Contains(key);
			}
		}

		[HarmonyPatch(typeof(KeyboardDevice))]
		[HarmonyPatch("IsKeyUp")]
		private class PatchKeyboardDeviceIsKeyUp
		{
			private static void Postfix(Key key, ref bool __result)
			{
				__result = !PressedKeys.Contains(key);
			}
		}

		[HarmonyPatch(typeof(KeyboardDevice))]
		[HarmonyPatch("get_Modifiers")]
		private class PatchKeyboardDeviceModifiers
		{
			private static void Postfix(ref ModifierKeys __result)
			{
				var modifiers = ModifierKeys.None;
				if (PressedKeys.Contains(Key.LeftShift) || PressedKeys.Contains(Key.RightShift))
					modifiers |= ModifierKeys.Shift;
				if (PressedKeys.Contains(Key.LeftCtrl) || PressedKeys.Contains(Key.RightCtrl))
					modifiers |= ModifierKeys.Control;
				if (PressedKeys.Contains(Key.LeftAlt) || PressedKeys.Contains(Key.RightAlt))
					modifiers |= ModifierKeys.Alt;
				__result = modifiers;
			}
		}

		#endregion
	}
}