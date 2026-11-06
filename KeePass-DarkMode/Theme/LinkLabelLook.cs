using System.ComponentModel;
using System.Drawing;
using KeePassDarkMode.Editor;

namespace KeePassDarkMode.Theme
{
	[TypeConverter(typeof(LinkLabelLookTypeConverter))]
	class LinkLabelLook : ControlLook
	{
		public Color LinkColor { get; set; }
	}
}