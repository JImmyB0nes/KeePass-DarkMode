using System;
using System.Drawing;
using Microsoft.Win32;

namespace KeePassDarkMode.Theme
{
	// Reads the current Windows accent color (the color shown in
	// Settings > Personalization > Colors). Used when the user enables
	// "Follow Windows accent color" so selections, links and highlights
	// pick up the system's own color instead of the theme's fixed one.
	internal static class AccentColor
	{
		public static Color Get()
		{
			try
			{
				using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
				{
					if (key != null)
					{
						var value = key.GetValue("ColorizationColor");
						if (value != null)
						{
							// DWORD format: 0xAABBGGRR - alpha is ignored.
							var argb = Convert.ToInt32(value);
							var color = Color.FromArgb(
								(argb >> 16) & 0xFF,   // R
								(argb >> 8) & 0xFF,    // G
								argb & 0xFF);          // B

							// A fully transparent or black value means the
							// DWM composition is off - fall back to the
							// classic highlight color.
							if ((argb & 0xFF000000) != 0 && !color.IsEmpty &&
							    (color.R > 0 || color.G > 0 || color.B > 0))
								return color;
						}
					}
				}
			}
			catch (Exception)
			{
				// Registry access blocked - fall through to the fallback.
			}

			return SystemColors.Highlight;
		}
	}
}
