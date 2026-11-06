using System.ComponentModel;
using KeePassDarkMode.Editor;

namespace KeePassDarkMode.Theme
{
    [TypeConverter(typeof(CheckBoxButtonLookTypeConverter))]
    class CheckBoxButtonLook : CheckBoxLook
    {
    }
}