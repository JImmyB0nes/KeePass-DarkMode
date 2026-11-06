using System.ComponentModel;
using KeePassDarkMode.Editor;

namespace KeePassDarkMode.Theme
{
	[TypeConverter(typeof(ScrollBarLookTypeConverter))]
	class ScrollBarLook
	{
		public bool UseExplorerDarkMode { get; set; }
		
		public override string ToString()
		{
			return GetType().Name;
		}
	}
}