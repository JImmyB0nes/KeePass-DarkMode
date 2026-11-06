using System.ComponentModel;
using System.Drawing;
using KeePassDarkMode.Editor;

namespace KeePassDarkMode.Theme
{
	[TypeConverter(typeof(OtherLookTypeConverter))]
	class OtherLook
	{
		public Color ControlNormalColor { get; set; }
		public Color ControlDisabledColor { get; set; }
		public Color ColorEditError { get; set; }
		
		public override string ToString()
		{
			return GetType().Name;
		}
	}
}