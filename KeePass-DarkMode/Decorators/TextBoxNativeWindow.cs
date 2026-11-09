using System;
using System.Drawing;
using System.Windows.Forms;
using KeePassDarkMode.Theme;

namespace KeePassDarkMode.Decorators
{
	// The standard EDIT window class registers COLOR_WINDOW+1 as its background
	// brush, so Windows erases the client area white whenever the control gains
	// or loses focus - one frame of white before our themed paint lands. That is
	// the flicker seen when a dialog opens and its first field takes focus.
	// Subclassing the control and filling WM_ERASEBKGND ourselves removes that
	// frame without touching KeePass' own painting.
	internal sealed class TextBoxNativeWindow : NativeWindow
	{
		private const int WM_ERASEBKGND = 0x0014;

		private readonly TextBoxBase _textBox;
		private ITheme _theme;
		private bool _enabled;

		internal TextBoxNativeWindow(TextBoxBase textBox, ITheme theme)
		{
			_textBox = textBox;
			_theme = theme;
			_enabled = true;

			_textBox.HandleCreated += OnHandleCreated;
			_textBox.HandleDestroyed += OnHandleDestroyed;
			_textBox.Disposed += OnDisposed;

			TryAssign(_textBox.IsHandleCreated ? _textBox.Handle : IntPtr.Zero);
		}

		internal void EnableTheme(bool enabled, ITheme theme)
		{
			if (_enabled == enabled && _theme == theme)
				return;

			_enabled = enabled;
			_theme = theme;

			if (!_textBox.IsDisposed)
				_textBox.Invalidate();
		}

		private void OnDisposed(object sender, EventArgs e)
		{
			_textBox.HandleCreated -= OnHandleCreated;
			_textBox.HandleDestroyed -= OnHandleDestroyed;
			ReleaseHandle();
		}

		private void OnHandleCreated(object sender, EventArgs e)
		{
			TryAssign(_textBox.Handle);
		}

		private void OnHandleDestroyed(object sender, EventArgs e)
		{
			ReleaseHandle();
		}

		private void TryAssign(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return;

			try
			{
				AssignHandle(handle);
			}
			catch (InvalidOperationException)
			{
				// Handle vanished between the check and the assignment.
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (_enabled && m.Msg == WM_ERASEBKGND)
			{
				// Deliberately no base call: DefWindowProc would fill with the
				// white class brush.
				using (var graphics = Graphics.FromHwnd(m.HWnd))
				using (var background = new SolidBrush(_theme.Control.BackColor))
					graphics.FillRectangle(background, _textBox.ClientRectangle);

				m.Result = IntPtr.One;
				return;
			}

			base.WndProc(ref m);
		}
	}
}
