using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeePassDarkMode.Editor;

namespace KeePassDarkMode.Theme
{
	[TypeConverter(typeof(RichTextBoxLookTypeConverter))]
	class RichTextBoxLook : ControlLook
	{
		public BorderStyle BorderStyle { get; set; }
		public Color SelectionColor { get; set; }
	}
}