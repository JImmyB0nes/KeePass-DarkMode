using System.Drawing;
using System.Windows.Forms;
using KeePass.UI.ToolStripRendering;

namespace KeePassDarkMode.Theme
{
	class CustomToolStripRenderer : ProExtTsr
	{
		private readonly CustomTheme _customTheme;

		protected override bool EnsureTextContrast
		{
			get { return false; }
		}

		public CustomToolStripRenderer(CustomTheme customTheme, ProfessionalColorTable ct) : base(ct)
		{
			_customTheme = customTheme;
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			if (e.Item.Pressed || e.Item.Selected)
			{
				e.TextColor = _customTheme.MenuItem.HighlightColor;
			}
			else if (e.Item.Enabled)
			{
				// Items that KeePass creates on the fly (entry views, "Open with
				// browser", templates) keep the default dark gray ForeColor and
				// sink into the dark menu. Any dark text on our dark background
				// becomes the theme's menu text color.
				var color = e.TextColor;
				if (color.GetBrightness() < 0.45)
					e.TextColor = _customTheme.MenuItem.ForeColor;
			}

			base.OnRenderItemText(e);
		}

		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			var ms = e.ToolStrip as MenuStrip;
			if (ms != null) 
			{
				using (var menuBackgroundBrush = new SolidBrush(_customTheme.MenuItem.BackColor))
				{
					e.Graphics.FillRectangle(menuBackgroundBrush, e.AffectedBounds);
				}
			} 
			else 
			{
				base.OnRenderToolStripBackground(e);
			}
		}
	}
}
