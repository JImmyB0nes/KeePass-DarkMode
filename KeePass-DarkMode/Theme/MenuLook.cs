using System.ComponentModel;
using System.Drawing;
using KeePassDarkMode.Editor;

namespace KeePassDarkMode.Theme
{
	[TypeConverter(typeof(MenuLookTypeConverter))]
	class MenuLook : ControlLook
	{
		public Color HighlightColor { get; set; }
	}
}