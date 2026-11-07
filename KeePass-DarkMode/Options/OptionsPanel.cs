using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using KeePass.Forms;
using KeePass.UI;
using KeePass.Util;
using KeePassDarkMode.Editor;
using KeePassDarkMode.Theme;

namespace KeePassDarkMode.Options
{
	public partial class OptionsPanel : UserControl
	{
		private const string ReleaseApiUrl = "https://api.github.com/repos/JImmyB0nes/KeePass-DarkMode/releases/latest";
		private const string ReleasesUrl = "https://github.com/JImmyB0nes/KeePass-DarkMode/releases";

		private readonly DarkModeOptions _options;

		public OptionsPanel()
		{
			InitializeComponent();

			if (!WinUtil.IsAtLeastWindows10)
				autoSyncWin10ThemeCheckBox.Enabled = false;
		}

		private OptionsPanel(DarkModeOptions options) : this()
		{
			_options = options;

			LoadOptions();
		}

		public static void Create(OptionsForm optionsForm, DarkModeOptions options)
		{
			var controls = optionsForm.Controls.Find("m_tabMain", false);
			if (controls.Length != 1)
				return;

			var tabControl = controls[0] as TabControl;
			if (tabControl == null)
				return;

			if (tabControl.ImageList == null)
			{
				tabControl.ImageList = new ImageList();
				tabControl.ImageList.ImageSize = new Size(DpiUtil.ScaleIntX(16), DpiUtil.ScaleIntY(16));
			}
			var imageIndex = tabControl.ImageList.Images.Add(Properties.Resource.PluginIcon, Color.Transparent);
			
			var optionsPanel = new OptionsPanel(options);
			var tabPage = new TabPage("KeePass-DarkMode");
			tabPage.ImageIndex = imageIndex;
			tabPage.Controls.Add(optionsPanel);
			tabPage.UseVisualStyleBackColor = true;
			tabControl.TabPages.Add(tabPage);
			optionsPanel.Dock = DockStyle.Fill;

			if (TemplateEditorForm.Instance != null)
			{
				TemplateEditorForm.Instance.PreviewButtonClick += optionsPanel.HandleTemplateEditorPreviewButtonClick;
				TemplateEditorForm.Instance.Closed += optionsPanel.HandleTemplateEditorPreviewButtonClick;
			}

			optionsForm.FormClosed += (sender, args) =>
			{
				if (optionsForm.DialogResult == DialogResult.OK)
					optionsPanel.SaveOptions();
				
				if (TemplateEditorForm.Instance != null)
				{
					TemplateEditorForm.Instance.PreviewButtonClick -= optionsPanel.HandleTemplateEditorPreviewButtonClick;
					TemplateEditorForm.Instance.Closed -= optionsPanel.HandleTemplateEditorPreviewButtonClick;
				}
			};
		}

		private void LoadOptions()
		{
			hotKeyTextBox.HotKey = _options.HotKey;
			autoSyncWin10ThemeCheckBox.Checked = _options.AutoSyncWithWin10Theme;
			autoLightThemeCheckBox.Checked = _options.AutoLightTheme;
			accentFromWindowsCheckBox.Checked = _options.AccentFromWindows;
			checkUpdatesCheckBox.Checked = _options.CheckUpdates;
			LoadDarkModeTemplates();
			UpdateThemePreview();
			LoadVersionInfo();
		}

		private void LoadVersionInfo()
		{
			versionLink.Text = "Version " + UpdateChecker.CurrentVersion.ToString(3);
			versionLink.LinkArea = new LinkArea(0, 0); // plain text until update known

			// Background check against the GitHub releases API - never blocks the dialog.
			var worker = new System.ComponentModel.BackgroundWorker();
			worker.DoWork += (s, args) => args.Result = UpdateChecker.QueryLatestVersion();
			worker.RunWorkerCompleted += HandleVersionCheckCompleted;
			worker.RunWorkerAsync();
		}

		private static Version QueryLatestVersion()
		{
			return UpdateChecker.QueryLatestVersion();
		}

		private void HandleVersionCheckCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs args)
		{
			var latest = args.Error == null ? args.Result as Version : null;
			if (latest == null || latest <= UpdateChecker.CurrentVersion)
				return;

			versionLink.Text = string.Format("Version {0} - Update {1} available!",
				UpdateChecker.CurrentVersion.ToString(3), latest.ToString(3));
			versionLink.LinkArea = new LinkArea(versionLink.Text.IndexOf('-'),
				versionLink.Text.Length - versionLink.Text.IndexOf('-'));
		}

		private void HandleVersionLinkClicked(object sender, LinkLabelLinkClickedEventArgs args)
		{
			try
			{
				System.Diagnostics.Process.Start(ReleasesUrl);
			}
			catch (Exception)
			{
				// Blocked by policy - nothing else we can do.
			}
		}

		private void HandleTemplateSelectionChanged(object sender, EventArgs e)
		{
			UpdateThemePreview();
		}

		private void UpdateThemePreview()
		{
			var template = (TemplateFile) themeTemplateComboBox.SelectedItem;
			if (template == null)
				return;

			try
			{
				var iniFile = TemplateReader.Get(template.Path);
				if (iniFile != null)
					themePreview.ApplyTheme(new CustomThemeTemplate(iniFile));
			}
			catch (Exception)
			{
				// Preview is optional - never fail the dialog over it.
			}
		}

		private void LoadDarkModeTemplates()
		{
			var templates = TemplateReader.GetTemplatesFromResources();
			var fileTemplates = TemplateReader.GetTemplatesFromPluginsDir();
			templates.AddRange(fileTemplates);

			var selectedTemplate = templates.Find(x => x.Path == _options.Template);
			if (selectedTemplate == null)
			{
				var templateName = TemplateReader.GetTemplateName(_options.Template);
				if (templateName != null)
				{
					var templateFile = new TemplateFile();
					templateFile.Name = templateName;
					templateFile.Path = _options.Template;
					templates.Add(templateFile);
					selectedTemplate = templateFile;
				}
			}

			themeTemplateComboBox.Items.Clear();
			foreach (var template in templates)
			{
				themeTemplateComboBox.Items.Add(template);
			}

			// First would throw if the default template is not in the list (e.g. after
			// a partial plugin update) - use FirstOrDefault and fall back gracefully.
			themeTemplateComboBox.SelectedItem = selectedTemplate
				?? templates.FirstOrDefault(x => x.Path == TemplateReader.DefaultTemplatePath)
				?? templates.FirstOrDefault();
		}

		private void SaveOptions()
		{
			_options.HotKey = hotKeyTextBox.HotKey;
			_options.AutoSyncWithWin10Theme = autoSyncWin10ThemeCheckBox.Checked;
			_options.AutoLightTheme = autoLightThemeCheckBox.Checked;
			_options.AccentFromWindows = accentFromWindowsCheckBox.Checked;
			_options.CheckUpdates = checkUpdatesCheckBox.Checked;

			var template = (TemplateFile) themeTemplateComboBox.SelectedItem;
			if (template != null)
				_options.Template = template.Path;
		}

		private void HandleEditTemplateButtonClick(object sender, EventArgs e)
		{
			if (TemplateEditorForm.Instance == null)
			{
				var templateEditorForm = TemplateEditorForm.Create(_options);
				if (templateEditorForm == null)
					return;

				templateEditorForm.PreviewButtonClick += HandleTemplateEditorPreviewButtonClick;
				templateEditorForm.Closed += HandleTemplateEditorFormClosed;
				templateEditorForm.Show();
				templateEditorForm.Activate();
			}
		}

		private void HandleTemplateEditorFormClosed(object sender, EventArgs e)
		{
			LoadDarkModeTemplates();
		}

		private void HandleTemplateEditorPreviewButtonClick(object sender, EventArgs e)
		{
			LoadDarkModeTemplates();
		}

		private void HandleOpenThemeFolderButtonClick(object sender, EventArgs e)
		{
			try
			{
				var dir = TemplateReader.GetTemplatesDir();
				if (!System.IO.Directory.Exists(dir))
					System.IO.Directory.CreateDirectory(dir);
				System.Diagnostics.Process.Start("explorer.exe", "\"" + dir + "\"");
			}
			catch (Exception)
			{
				// Blocked by policy - nothing else we can do.
			}
		}

		private void HandleExportSettingsButtonClick(object sender, EventArgs e)
		{
			using (var dialog = new SaveFileDialog())
			{
				dialog.Title = "Export KeePass-DarkMode settings";
				dialog.Filter = "KeePass-DarkMode settings (*.kdmconfig)|*.kdmconfig";
				dialog.FileName = "KeePass-DarkMode.kdmconfig";
				if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
					return;

				try
				{
					System.IO.File.WriteAllText(dialog.FileName, _options.ExportSettings());
				}
				catch (Exception ex)
				{
					MessageBox.Show(FindForm(), "Could not export settings: " + ex.Message,
						"KeePass-DarkMode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}

	}
}