using System.ComponentModel;
using System.Drawing;
using KeePassDarkMode.Editor;

namespace KeePassDarkMode.Theme
{
	[TypeConverter(typeof(MenuLookTypeConverter))]
	class MenuLook : ControlLook
	{
		public Color HighlightColor { get; set; }

		// Draws vector toolbar/menu glyphs in theme color instead of the
		// classic colorful KeePass icons.
		public bool ModernIcons { get; set; }
	}
}