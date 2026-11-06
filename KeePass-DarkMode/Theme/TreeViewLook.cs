using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeePassDarkMode.Editor;

namespace KeePassDarkMode.Theme
{
	[TypeConverter(typeof(TreeViewLookTypeConverter))]
	class TreeViewLook : ControlLook
	{
		public Color SelectionColor { get; set; }
		public Color SelectionBackColor { get; set; }
		public BorderStyle BorderStyle { get; set; }
	}
}