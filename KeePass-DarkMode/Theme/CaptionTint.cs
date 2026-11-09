using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeePassDarkMode.Theme
{
	// Colors the DWM title bar of a form to match the theme (Win11 22H2+;
	// silently ignored on older systems where the attributes don't exist).
	internal static class CaptionTint
	{
		private const int DWMWA_CAPTION_COLOR = 35;
		private const int DWMWA_TEXT_COLOR = 36;

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

		public static void Apply(Form form, Color backColor, Color foreColor, bool dark)
		{
			if (!form.IsHandleCreated)
			{
				// Apply(Form) runs repeatedly (every Visit/theme change) while the
				// handle may still be missing - hook exactly once per form and
				// apply only the latest requested colors.
				var alreadyHooked = Pending.ContainsKey(form);
				Pending[form] = new PendingTint(backColor, foreColor, dark);
				if (alreadyHooked)
					return;

				EventHandler hook = null;
				hook = (s, e) =>
				{
					form.HandleCreated -= hook;
					PendingTint tint;
					if (!Pending.TryGetValue(form, out tint))
						return;
					Pending.Remove(form);
					Apply(form, tint.BackColor, tint.ForeColor, tint.Dark);
				};
				form.HandleCreated += hook;
				form.Disposed += (s, e) => Pending.Remove(form);
				return;
			}

			try
			{
				if (!dark || backColor == Color.Empty)
				{
					// -1 resets the attribute to the system default.
					var reset = -1;
					DwmSetWindowAttribute(form.Handle, DWMWA_CAPTION_COLOR, ref reset, sizeof(int));
					DwmSetWindowAttribute(form.Handle, DWMWA_TEXT_COLOR, ref reset, sizeof(int));
					return;
				}

				var caption = ToColorRef(backColor);
				var text = ToColorRef(foreColor == Color.Empty ? Color.White : foreColor);
				DwmSetWindowAttribute(form.Handle, DWMWA_CAPTION_COLOR, ref caption, sizeof(int));
				DwmSetWindowAttribute(form.Handle, DWMWA_TEXT_COLOR, ref text, sizeof(int));
			}
			catch (Exception)
			{
				// dwmapi unavailable or attribute unsupported - skip silently.
			}
		}

		private sealed class PendingTint
		{
			public readonly Color BackColor;
			public readonly Color ForeColor;
			public readonly bool Dark;

			public PendingTint(Color backColor, Color foreColor, bool dark)
			{
				BackColor = backColor;
				ForeColor = foreColor;
				Dark = dark;
			}
		}

		// UI thread only - no locking needed.
		private static readonly System.Collections.Generic.Dictionary<Form, PendingTint> Pending =
			new System.Collections.Generic.Dictionary<Form, PendingTint>();

		// COLORREF is 0x00BBGGRR.
		private static int ToColorRef(Color color)
		{
			return color.B << 16 | color.G << 8 | color.R;
		}
	}
}
