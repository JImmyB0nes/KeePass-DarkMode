using System;
using System.Drawing;
using System.Reflection;
using KeePassDarkMode.Theme;

namespace KeePassDarkMode.Decorators
{
    internal static class KnownColorsDecorator
    {
        private static int[] _originalColorTable;
        
        public static void Apply(ITheme theme, bool enabled)
        {
            var colorTableField = typeof(Color).Assembly.GetType("System.Drawing.KnownColorTable")
                .GetField("colorTable", BindingFlags.Static | BindingFlags.NonPublic);

            if (colorTableField == null)
                return;
			
            var colorTable = (int[]) colorTableField.GetValue(null);
            if (_originalColorTable == null)
            {
                _originalColorTable = new int[colorTable.Length];
                Array.Copy(colorTable, _originalColorTable, colorTable.Length);
            }
            
            // Always start from the original system colors, so a theme without
            // Control colors (Color.Empty) never writes 0 (transparent black).
            Array.Copy(_originalColorTable, colorTable, colorTable.Length);

            if (enabled)
            {
                if (!theme.Control.ForeColor.IsEmpty)
                    colorTable[(int) KnownColor.ControlText] = theme.Control.ForeColor.ToArgb();
                if (!theme.Control.BackColor.IsEmpty)
                    colorTable[(int) KnownColor.Control] = theme.Control.BackColor.ToArgb();
            }
        }
    }
}
