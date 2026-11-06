using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeePassDarkMode.Editor;

namespace KeePassDarkMode.Theme
{
	[TypeConverter(typeof(ButtonLookTypeConverter))]
	class ButtonLook : ControlLook
	{
		public Color BorderColor { get; set; }
		public FlatStyle FlatStyle { get; set; }
	}
}