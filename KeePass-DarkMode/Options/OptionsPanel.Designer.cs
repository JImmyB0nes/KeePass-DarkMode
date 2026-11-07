using System.ComponentModel;
using System.Windows.Forms;

namespace KeePassDarkMode.Options
{
	partial class OptionsPanel
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.themeGroup = new System.Windows.Forms.GroupBox();
			this.behaviorGroup = new System.Windows.Forms.GroupBox();
			this.hotKeyLabel = new System.Windows.Forms.Label();
			this.hotKeyTextBox = new KeePass.UI.HotKeyControlEx();
			this.autoSyncWin10ThemeCheckBox = new System.Windows.Forms.CheckBox();
			this.autoLightThemeCheckBox = new System.Windows.Forms.CheckBox();
			this.accentFromWindowsCheckBox = new System.Windows.Forms.CheckBox();
			this.themeTemplate = new System.Windows.Forms.Label();
			this.themeTemplateComboBox = new System.Windows.Forms.ComboBox();
			this.editTemplateButton = new System.Windows.Forms.Button();
			this.versionLink = new System.Windows.Forms.LinkLabel();
			this.checkUpdatesCheckBox = new System.Windows.Forms.CheckBox();
			this.themePreview = new KeePassDarkMode.Options.ThemePreviewControl();
			this.previewLabel = new System.Windows.Forms.Label();
			this.openThemeFolderButton = new System.Windows.Forms.Button();
			this.exportSettingsButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// themeGroup
			// 
			this.themeGroup.Controls.Add(this.themeTemplate);
			this.themeGroup.Controls.Add(this.themeTemplateComboBox);
			this.themeGroup.Controls.Add(this.editTemplateButton);
			this.themeGroup.Controls.Add(this.openThemeFolderButton);
			this.themeGroup.Controls.Add(this.exportSettingsButton);
			this.themeGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
										| System.Windows.Forms.AnchorStyles.Right)));
			this.themeGroup.Location = new System.Drawing.Point(12, 9);
			this.themeGroup.Name = "themeGroup";
			this.themeGroup.Size = new System.Drawing.Size(432, 122);
			this.themeGroup.TabIndex = 0;
			this.themeGroup.TabStop = false;
			this.themeGroup.Text = "Theme";
			// 
			// themeTemplate
			// 
			this.themeTemplate.Location = new System.Drawing.Point(12, 25);
			this.themeTemplate.Name = "themeTemplate";
			this.themeTemplate.Size = new System.Drawing.Size(70, 23);
			this.themeTemplate.TabIndex = 0;
			this.themeTemplate.Text = "Template:";
			// 
			// themeTemplateComboBox
			// 
			this.themeTemplateComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
										| System.Windows.Forms.AnchorStyles.Right)));
			this.themeTemplateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.themeTemplateComboBox.FormattingEnabled = true;
			this.themeTemplateComboBox.Location = new System.Drawing.Point(88, 23);
			this.themeTemplateComboBox.Name = "themeTemplateComboBox";
			this.themeTemplateComboBox.Size = new System.Drawing.Size(331, 21);
			this.themeTemplateComboBox.TabIndex = 1;
			this.themeTemplateComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleTemplateSelectionChanged);
			// 
			// editTemplateButton
			// 
			this.editTemplateButton.Location = new System.Drawing.Point(12, 52);
			this.editTemplateButton.Name = "editTemplateButton";
			this.editTemplateButton.Size = new System.Drawing.Size(200, 27);
			this.editTemplateButton.TabIndex = 2;
			this.editTemplateButton.Text = "Open Editor";
			this.editTemplateButton.UseVisualStyleBackColor = true;
			this.editTemplateButton.Click += new System.EventHandler(this.HandleEditTemplateButtonClick);
			// 
			// openThemeFolderButton
			// 
			this.openThemeFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.openThemeFolderButton.Location = new System.Drawing.Point(219, 52);
			this.openThemeFolderButton.Name = "openThemeFolderButton";
			this.openThemeFolderButton.Size = new System.Drawing.Size(200, 27);
			this.openThemeFolderButton.TabIndex = 3;
			this.openThemeFolderButton.Text = "Theme folder";
			this.openThemeFolderButton.UseVisualStyleBackColor = true;
			this.openThemeFolderButton.Click += new System.EventHandler(this.HandleOpenThemeFolderButtonClick);
			// 
			// exportSettingsButton
			// 
			this.exportSettingsButton.Location = new System.Drawing.Point(12, 85);
			this.exportSettingsButton.Name = "exportSettingsButton";
			this.exportSettingsButton.Size = new System.Drawing.Size(200, 27);
			this.exportSettingsButton.TabIndex = 5;
			this.exportSettingsButton.Text = "Export settings";
			this.exportSettingsButton.UseVisualStyleBackColor = true;
			this.exportSettingsButton.Click += new System.EventHandler(this.HandleExportSettingsButtonClick);
			// 
			// behaviorGroup
			// 
			this.behaviorGroup.Controls.Add(this.hotKeyLabel);
			this.behaviorGroup.Controls.Add(this.hotKeyTextBox);
			this.behaviorGroup.Controls.Add(this.autoSyncWin10ThemeCheckBox);
			this.behaviorGroup.Controls.Add(this.autoLightThemeCheckBox);
			this.behaviorGroup.Controls.Add(this.accentFromWindowsCheckBox);
			this.behaviorGroup.Controls.Add(this.checkUpdatesCheckBox);
			this.behaviorGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
										| System.Windows.Forms.AnchorStyles.Right)));
			this.behaviorGroup.Location = new System.Drawing.Point(12, 141);
			this.behaviorGroup.Name = "behaviorGroup";
			this.behaviorGroup.Size = new System.Drawing.Size(432, 176);
			this.behaviorGroup.TabIndex = 1;
			this.behaviorGroup.TabStop = false;
			this.behaviorGroup.Text = "Behavior";
			// 
			// hotKeyLabel
			// 
			this.hotKeyLabel.Location = new System.Drawing.Point(12, 26);
			this.hotKeyLabel.Name = "hotKeyLabel";
			this.hotKeyLabel.Size = new System.Drawing.Size(190, 23);
			this.hotKeyLabel.TabIndex = 0;
			this.hotKeyLabel.Text = "Toggle hot key:";
			// 
			// hotKeyTextBox
			// 
			this.hotKeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
										| System.Windows.Forms.AnchorStyles.Right)));
			this.hotKeyTextBox.Location = new System.Drawing.Point(208, 28);
			this.hotKeyTextBox.Name = "hotKeyTextBox";
			this.hotKeyTextBox.Size = new System.Drawing.Size(211, 20);
			this.hotKeyTextBox.TabIndex = 1;
			// 
			// autoSyncWin10ThemeCheckBox
			// 
			this.autoSyncWin10ThemeCheckBox.AutoSize = true;
			this.autoSyncWin10ThemeCheckBox.Location = new System.Drawing.Point(15, 58);
			this.autoSyncWin10ThemeCheckBox.Name = "autoSyncWin10ThemeCheckBox";
			this.autoSyncWin10ThemeCheckBox.Size = new System.Drawing.Size(210, 24);
			this.autoSyncWin10ThemeCheckBox.TabIndex = 2;
			this.autoSyncWin10ThemeCheckBox.Text = "Auto-sync with Windows theme";
			this.autoSyncWin10ThemeCheckBox.UseVisualStyleBackColor = true;
			// 
			// autoLightThemeCheckBox
			// 
			this.autoLightThemeCheckBox.AutoSize = true;
			this.autoLightThemeCheckBox.Location = new System.Drawing.Point(15, 88);
			this.autoLightThemeCheckBox.Name = "autoLightThemeCheckBox";
			this.autoLightThemeCheckBox.TabIndex = 3;
			this.autoLightThemeCheckBox.Text = "Follow Windows light/dark mode";
			this.autoLightThemeCheckBox.UseVisualStyleBackColor = true;
			// 
			// accentFromWindowsCheckBox
			// 
			this.accentFromWindowsCheckBox.AutoSize = true;
			this.accentFromWindowsCheckBox.Location = new System.Drawing.Point(15, 118);
			this.accentFromWindowsCheckBox.Name = "accentFromWindowsCheckBox";
			this.accentFromWindowsCheckBox.TabIndex = 4;
			this.accentFromWindowsCheckBox.Text = "Use Windows accent color";
			this.accentFromWindowsCheckBox.UseVisualStyleBackColor = true;
			// 
			// checkUpdatesCheckBox
			// 
			this.checkUpdatesCheckBox.AutoSize = true;
			this.checkUpdatesCheckBox.Location = new System.Drawing.Point(15, 148);
			this.checkUpdatesCheckBox.Name = "checkUpdatesCheckBox";
			this.checkUpdatesCheckBox.TabIndex = 5;
			this.checkUpdatesCheckBox.Text = "Check for updates (GitHub)";
			this.checkUpdatesCheckBox.UseVisualStyleBackColor = true;
			// 
			// previewLabel
			// 
			this.previewLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
			this.previewLabel.AutoSize = true;
			this.previewLabel.Location = new System.Drawing.Point(509, 15);
			this.previewLabel.Name = "previewLabel";
			this.previewLabel.Size = new System.Drawing.Size(74, 13);
			this.previewLabel.TabIndex = 2;
			this.previewLabel.Text = "Live preview";
			// 
			// themePreview
			// 
			this.themePreview.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
			this.themePreview.Location = new System.Drawing.Point(500, 31);
			this.themePreview.Name = "themePreview";
			this.themePreview.Size = new System.Drawing.Size(255, 220);
			this.themePreview.TabIndex = 3;
			// 
			// versionLink
			// 
			this.versionLink.AutoSize = true;
			this.versionLink.Location = new System.Drawing.Point(12, 335);
			this.versionLink.Name = "versionLink";
			this.versionLink.Size = new System.Drawing.Size(0, 13);
			this.versionLink.TabIndex = 4;
			this.versionLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleVersionLinkClicked);
			// 
			// OptionsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.themeGroup);
			this.Controls.Add(this.behaviorGroup);
			this.Controls.Add(this.previewLabel);
			this.Controls.Add(this.themePreview);
			this.Controls.Add(this.versionLink);
			this.Name = "OptionsPanel";
			this.Size = new System.Drawing.Size(767, 360);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.Windows.Forms.GroupBox themeGroup;
		private System.Windows.Forms.GroupBox behaviorGroup;
		private System.Windows.Forms.Button editTemplateButton;

		private System.Windows.Forms.LinkLabel versionLink;

		private System.Windows.Forms.CheckBox checkUpdatesCheckBox;

		private KeePassDarkMode.Options.ThemePreviewControl themePreview;
		private System.Windows.Forms.Label previewLabel;
		private System.Windows.Forms.Button openThemeFolderButton;
		private System.Windows.Forms.Button exportSettingsButton;

		private System.Windows.Forms.Label themeTemplate;
		private System.Windows.Forms.ComboBox themeTemplateComboBox;

		private System.Windows.Forms.CheckBox autoSyncWin10ThemeCheckBox;
		private System.Windows.Forms.CheckBox autoLightThemeCheckBox;
		private System.Windows.Forms.CheckBox accentFromWindowsCheckBox;

		private System.Windows.Forms.Label hotKeyLabel;
		private KeePass.UI.HotKeyControlEx hotKeyTextBox;

		#endregion
	}
}
