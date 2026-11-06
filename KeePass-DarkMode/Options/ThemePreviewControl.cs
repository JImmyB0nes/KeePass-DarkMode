using System;
using System.Drawing;
using System.Windows.Forms;
using KeePassDarkMode.Theme;

namespace KeePassDarkMode.Options
{
	// Miniature KeePass window rendered in the colors of the selected theme.
	internal class ThemePreviewControl : Panel
	{
		private readonly Label _titleBar;
		private readonly MenuStrip _menuStrip;
		private readonly TreeView _treeView;
		private readonly ListView _listView;
		private readonly TextBox _textBox;
		private readonly Button _button;
		private readonly LinkLabel _linkLabel;
		private readonly StatusStrip _statusStrip;

		public ThemePreviewControl()
		{
			BorderStyle = BorderStyle.FixedSingle;

			_titleBar = new Label
			{
				Text = " MyDatabase.kdbx - KeePass",
				Height = 20,
				Dock = DockStyle.Top,
				TextAlign = ContentAlignment.MiddleLeft,
				Font = new Font(Font.FontFamily, 8.5f)
			};

			_menuStrip = new MenuStrip
			{
				Dock = DockStyle.Top
			};
			_menuStrip.Items.Add(new ToolStripMenuItem("File"));
			_menuStrip.Items.Add(new ToolStripMenuItem("Edit"));
			_menuStrip.Items.Add(new ToolStripMenuItem("Tools"));

			var split = new SplitContainer
			{
				Dock = DockStyle.Fill,
				FixedPanel = SplitPanel.Panel1,
				FixedPanelWidth = 90
			};

			_treeView = new TreeView { Dock = DockStyle.Fill, HideSelection = false };
			_treeView.Nodes.Add("Root");
			_treeView.Nodes[0].Nodes.Add("General");
			_treeView.Nodes[0].Nodes.Add("Email");

			_listView = new ListView
			{
				Dock = DockStyle.Fill,
				View = View.Details,
				FullRowSelect = true
			};
			_listView.Columns.Add("Title", 80);
			_listView.Columns.Add("User", 60);
			_listView.Items.Add(new ListViewItem(new[] { "Sample Entry", "user1" }));
			_listView.Items.Add(new ListViewItem(new[] { "Sample Entry #2", "user2" }));

			var rightPanel = new Panel { Dock = DockStyle.Fill };
			rightPanel.Controls.Add(_listView);

			_textBox = new TextBox
			{
				Dock = DockStyle.Bottom,
				Text = "Search...",
				Height = 21
			};

			var bottomPanel = new FlowLayoutPanel
			{
				Dock = DockStyle.Bottom,
				Height = 30,
				FlowDirection = FlowDirection.LeftToRight
			};
			_button = new Button { Text = "OK", Size = new Size(60, 23) };
			_linkLabel = new LinkLabel
			{
				Text = "keepass.info",
				AutoSize = true,
				Margin = new Padding(10, 5, 0, 0)
			};
			bottomPanel.Controls.Add(_button);
			bottomPanel.Controls.Add(_linkLabel);

			_statusStrip = new StatusStrip
			{
				Dock = DockStyle.Bottom,
				Height = 18
			};
			_statusStrip.Items.Add(new ToolStripStatusLabel("Ready"));

			split.Panel1.Controls.Add(_treeView);
			split.Panel2.Controls.Add(rightPanel);
			split.Panel2.Controls.Add(bottomPanel);

			Controls.Add(split);
			Controls.Add(_menuStrip);
			Controls.Add(_textBox);
			Controls.Add(_statusStrip);
			Controls.Add(_titleBar);
		}

		public void ApplyTheme(CustomThemeTemplate theme)
		{
			var form = theme.Form;
			var control = theme.Control;
			var tree = theme.TreeView;
			var listView = theme.ListView;
			// ToolStripLook carries no plain colors (gradient renderer), so the
			// menu colors stand in for tool/status strips in the preview.
			var strip = theme.MenuItem;

			BackColor = form.BackColor;
			ForeColor = form.ForeColor;

			_titleBar.BackColor = strip.BackColor;
			_titleBar.ForeColor = strip.ForeColor;

			_menuStrip.BackColor = strip.BackColor;
			_menuStrip.ForeColor = strip.ForeColor;

			_treeView.BackColor = tree.BackColor;
			_treeView.ForeColor = tree.ForeColor;
			_treeView.HideSelection = false;

			_listView.BackColor = listView.BackColor;
			_listView.ForeColor = listView.ForeColor;

			_textBox.BackColor = control.BackColor;
			_textBox.ForeColor = control.ForeColor;

			_button.BackColor = theme.Button.BackColor;
			_button.ForeColor = theme.Button.ForeColor;

			_linkLabel.LinkColor = theme.LinkLabel.LinkColor;
			_linkLabel.ForeColor = control.ForeColor;

			_statusStrip.BackColor = strip.BackColor;
			_statusStrip.ForeColor = strip.ForeColor;
		}
	}
}
