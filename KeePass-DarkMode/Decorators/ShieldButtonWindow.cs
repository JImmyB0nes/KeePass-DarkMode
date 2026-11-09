using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace KeePassDarkMode.Decorators
{
	// KeePass marks elevation-capable buttons ("OK", "Create Association", ...)
	// with a UAC shield via BCM_SETSHIELD and forces FlatStyle.System, which
	// makes the native white button win over our theme colors. We intercept the
	// message, keep the button flat/dark while the theme is active and expose
	// the shield flag so the paint handler can draw shield + text in theme colors.
	internal sealed class ShieldButtonWindow : NativeWindow
	{
		private const int BCM_SETSHIELD = 0x1602;

		// winuser.h: IDI_SHIELD = MAKEINTRESOURCE(133)
		private static readonly IntPtr IDI_SHIELD = new IntPtr(133);

		[DllImport("user32.dll")]
		private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

		[DllImport("user32.dll", EntryPoint = "SendMessageW")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool DrawIconEx(IntPtr hdc, int xLeft, int yTop, IntPtr hIcon,
			int cxWidth, int cyWidth, uint istepIfAniCur, IntPtr hbrFlickerFreeDraw, uint diFlags);

		private const uint DI_NORMAL = 0x3;

		public static void DrawShield(Graphics graphics, Rectangle bounds)
		{
			var icon = GetShieldIcon();
			if (icon == IntPtr.Zero)
				return;

			try
			{
				var size = Math.Min(16, Math.Min(bounds.Width, bounds.Height));
				if (size < 8)
					return;

				var x = bounds.Left + 4;
				var y = bounds.Top + (bounds.Height - size) / 2;
				var hdc = graphics.GetHdc();
				try
				{
					DrawIconEx(hdc, x, y, icon, size, size, 0, IntPtr.Zero, DI_NORMAL);
				}
				finally
				{
					graphics.ReleaseHdc(hdc);
				}
			}
			catch (Exception)
			{
				// Decorative only.
			}
		}

		private readonly Button _button;
		private bool _enabled;

		public bool ShieldRequested { get; private set; }

		public ShieldButtonWindow(Button button)
		{
			_button = button;
			if (button.IsHandleCreated)
				AssignHandle(button.Handle);

			// Changing FlatStyle from/to System recreates the HWND - follow it,
			// otherwise BCM_SETSHIELD on the new handle would be missed.
			button.HandleCreated += (s, e) =>
			{
				if (Handle != button.Handle)
				{
					if (Handle != IntPtr.Zero)
						ReleaseHandle();
					AssignHandle(button.Handle);
				}
			};
		}

		public void SetEnabled(bool enabled)
		{
			if (_enabled == enabled)
				return;

			_enabled = enabled;
			if (enabled)
			{
				// Our flat painting takes over.
				if (_button.FlatStyle != FlatStyle.Flat)
					_button.FlatStyle = FlatStyle.Flat;
			}
			else if (ShieldRequested)
			{
				// Theme off: give KeePass its native shield button back - the
				// shield message was swallowed while the theme was active.
				_button.FlatStyle = FlatStyle.System;
				if (_button.IsHandleCreated)
					SendMessage(_button.Handle, BCM_SETSHIELD, IntPtr.Zero, new IntPtr(1));
			}
			_button.Invalidate();
		}

		public static IntPtr GetShieldIcon()
		{
			return LoadIcon(IntPtr.Zero, IDI_SHIELD);
		}

		// LoadIcon returns a shared system icon - it must NOT be destroyed.

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == BCM_SETSHIELD)
			{
				ShieldRequested = m.WParam != IntPtr.Zero;

				if (_enabled)
				{
					// Theme active: swallow the message, our paint draws the shield.
					if (_button.FlatStyle != FlatStyle.Flat)
						_button.FlatStyle = FlatStyle.Flat;
					_button.Invalidate();
					return;
				}

				if (ShieldRequested && _button.FlatStyle != FlatStyle.System)
				{
					// Theme off: native shield needs FlatStyle.System. That
					// recreates the HWND, so re-send the message to the new one.
					_button.FlatStyle = FlatStyle.System;
					if (_button.IsHandleCreated && _button.Handle != m.HWnd)
					{
						SendMessage(_button.Handle, BCM_SETSHIELD, m.WParam, m.LParam);
						return;
					}
				}

				// Theme off: let the native button show/hide its shield.
			}

			base.WndProc(ref m);
		}
	}
}
