using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeePassDarkMode.Decorators
{
	// KeePass shows native Win32 dialogs (message boxes, security warnings,
	// the Windows credential picker) that our control visitor never sees.
	// A global WinEvent hook catches every dialog window the moment it is
	// shown and applies the immersive dark title bar - the same trick the
	// main forms use, so message boxes finally match the theme.
	internal class NativeDialogWatcher : IDisposable
	{
		private delegate IntPtr WinEventDelegate(IntPtr hWinEventHook, uint eventType,
			IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

		private const uint WINEVENT_OUTOFCONTEXT = 0;
		private const uint EVENT_OBJECT_SHOW = 0x8004;
		private const int OBJID_WINDOW = 0;

		private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
		private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

		[DllImport("user32.dll")]
		private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
			IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc,
			uint idProcess, uint idThread, uint flags);

		[DllImport("user32.dll")]
		private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

		private readonly WinEventDelegate _callback;
		private IntPtr _hook;
		private bool _dark;

		public NativeDialogWatcher()
		{
			// Keep a strong reference - the delegate must stay alive while hooked.
			_callback = HandleWinEvent;
		}

		public void Enable(bool dark)
		{
			_dark = dark;

			if (dark && _hook == IntPtr.Zero)
			{
				_hook = SetWinEventHook(EVENT_OBJECT_SHOW, EVENT_OBJECT_SHOW, IntPtr.Zero,
					_callback, 0, 0, WINEVENT_OUTOFCONTEXT);
			}
			else if (!dark && _hook != IntPtr.Zero)
			{
				UnhookWinEvent(_hook);
				_hook = IntPtr.Zero;
			}
		}

		private IntPtr HandleWinEvent(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
			int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
		{
			if (!_dark || idObject != OBJID_WINDOW || hwnd == IntPtr.Zero)
				return IntPtr.Zero;

			try
			{
				var className = new System.Text.StringBuilder(64);
				if (GetClassName(hwnd, className, 64) == 0)
					return IntPtr.Zero;

				// #32770 = dialog boxes (MessageBox, TaskDialog, credential prompts).
				if (className.ToString() == "#32770")
					SetDark(hwnd);
			}
			catch (Exception)
			{
				// Never let a hook callback throw across the WinEvent boundary.
			}

			return IntPtr.Zero;
		}

		private static void SetDark(IntPtr hwnd)
		{
			var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
			var value = 1;
			if (DwmSetWindowAttribute(hwnd, attribute, ref value, sizeof(int)) != 0)
			{
				// Older builds (17763..18985) use attribute 19.
				attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
				DwmSetWindowAttribute(hwnd, attribute, ref value, sizeof(int));
			}
		}

		public void Dispose()
		{
			if (_hook != IntPtr.Zero)
			{
				UnhookWinEvent(_hook);
				_hook = IntPtr.Zero;
			}
		}
	}
}
