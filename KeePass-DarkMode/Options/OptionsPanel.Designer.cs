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
			this.hotKeyLabel = new System.Windows.Forms.Label();
			this.hotKeyTextBox = new KeePass.UI.HotKeyControlEx();
			this.autoSyncWin10ThemeCheckBox = new System.Windows.Forms.CheckBox();
			this.autoSyncWin10ThemeLabel = new System.Windows.Forms.Label();
			this.themeTemplate = new System.Windows.Forms.Label();
			this.themeTemplateComboBox = new System.Windows.Forms.ComboBox();
			this.editTemplateButton = new System.Windows.Forms.Button();
			this.versionLink = new System.Windows.Forms.LinkLabel();
			this.checkUpdatesLabel = new System.Windows.Forms.Label();
			this.checkUpdatesCheckBox = new System.Windows.Forms.CheckBox();
			this.themePreview = new KeePassDarkMode.Options.ThemePreviewControl();
			this.openThemeFolderButton = new System.Windows.Forms.Button();
			this.importSettingsButton = new System.Windows.Forms.Button();
			this.exportSettingsButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// hotKeyLabel
			// 
			this.hotKeyLabel.Location = new System.Drawing.Point(9, 67);
			this.hotKeyLabel.Name = "hotKeyLabel";
			this.hotKeyLabel.Size = new System.Drawing.Size(110, 23);
			this.hotKeyLabel.TabIndex = 0;
			this.hotKeyLabel.Text = "KeePass-DarkMode hot key:";
			// 
			// hotKeyTextBox
			// 
			this.hotKeyTextBox.Location = new System.Drawing.Point(125, 64);
			this.hotKeyTextBox.Name = "hotKeyTextBox";
			this.hotKeyTextBox.Size = new System.Drawing.Size(166, 20);
			this.hotKeyTextBox.TabIndex = 1;
			// 
			// autoSyncWin10ThemeCheckBox
			// 
			this.autoSyncWin10ThemeCheckBox.Location = new System.Drawing.Point(189, 90);
			this.autoSyncWin10ThemeCheckBox.Name = "autoSyncWin10ThemeCheckBox";
			this.autoSyncWin10ThemeCheckBox.Size = new System.Drawing.Size(102, 24);
			this.autoSyncWin10ThemeCheckBox.TabIndex = 2;
			this.autoSyncWin10ThemeCheckBox.UseVisualStyleBackColor = true;
			// 
			// autoSyncWin10ThemeLabel
			// 
			this.autoSyncWin10ThemeLabel.Location = new System.Drawing.Point(9, 95);
			this.autoSyncWin10ThemeLabel.Name = "autoSyncWin10ThemeLabel";
			this.autoSyncWin10ThemeLabel.Size = new System.Drawing.Size(174, 23);
			this.autoSyncWin10ThemeLabel.TabIndex = 3;
			this.autoSyncWin10ThemeLabel.Text = "Auto-sync with Windows 10 theme:";
			// 
			// themeTemplate
			// 
			this.themeTemplate.Location = new System.Drawing.Point(9, 11);
			this.themeTemplate.Name = "themeTemplate";
			this.themeTemplate.Size = new System.Drawing.Size(110, 23);
			this.themeTemplate.TabIndex = 4;
			this.themeTemplate.Text = "KeePass-DarkMode template:";
			// 
			// themeTemplateComboBox
			// 
			this.themeTemplateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.themeTemplateComboBox.FormattingEnabled = true;
			this.themeTemplateComboBox.Location = new System.Drawing.Point(125, 8);
			this.themeTemplateComboBox.Name = "themeTemplateComboBox";
			this.themeTemplateComboBox.Size = new System.Drawing.Size(166, 21);
			this.themeTemplateComboBox.TabIndex = 6;
			this.themeTemplateComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleTemplateSelectionChanged);
			// 
			// editTemplateButton
			// 
			this.editTemplateButton.Location = new System.Drawing.Point(125, 35);
			this.editTemplateButton.Name = "editTemplateButton";
			this.editTemplateButton.Size = new System.Drawing.Size(166, 23);
			this.editTemplateButton.TabIndex = 7;
			this.editTemplateButton.Text = "Open Editor";
			this.editTemplateButton.UseVisualStyleBackColor = true;
			this.editTemplateButton.Click += new System.EventHandler(this.HandleEditTemplateButtonClick);
			// 
			// versionLink
			// 
			this.versionLink.AutoSize = true;
			this.versionLink.Location = new System.Drawing.Point(9, 170);
			// 
			// checkUpdatesLabel
			// 
			this.checkUpdatesLabel.Location = new System.Drawing.Point(9, 140);
			this.checkUpdatesLabel.Name = "checkUpdatesLabel";
			this.checkUpdatesLabel.Size = new System.Drawing.Size(174, 23);
			this.checkUpdatesLabel.TabIndex = 9;
			this.checkUpdatesLabel.Text = "Check for updates (GitHub):";
			// 
			// checkUpdatesCheckBox
			// 
			this.checkUpdatesCheckBox.Location = new System.Drawing.Point(189, 140);
			this.checkUpdatesCheckBox.Name = "checkUpdatesCheckBox";
			this.checkUpdatesCheckBox.Size = new System.Drawing.Size(102, 24);
			this.checkUpdatesCheckBox.TabIndex = 10;
			this.checkUpdatesCheckBox.UseVisualStyleBackColor = true;
			// 
			// openThemeFolderButton
			// 
			// 
			// themePreview
			// 
			this.themePreview.Location = new System.Drawing.Point(430, 8);
			this.themePreview.Name = "themePreview";
			this.themePreview.Size = new System.Drawing.Size(205, 185);
			this.themePreview.TabIndex = 14;
			// 
			// openThemeFolderButton
			// 
			this.openThemeFolderButton.Location = new System.Drawing.Point(311, 8);
			this.openThemeFolderButton.Name = "openThemeFolderButton";
			this.openThemeFolderButton.Size = new System.Drawing.Size(110, 23);
			this.openThemeFolderButton.TabIndex = 11;
			this.openThemeFolderButton.Text = "Theme folder";
			this.openThemeFolderButton.UseVisualStyleBackColor = true;
			this.openThemeFolderButton.Click += new System.EventHandler(this.HandleOpenThemeFolderButtonClick);
			// 
			// importSettingsButton
			// 
			this.importSettingsButton.Location = new System.Drawing.Point(311, 35);
			this.importSettingsButton.Name = "importSettingsButton";
			this.importSettingsButton.Size = new System.Drawing.Size(110, 23);
			this.importSettingsButton.TabIndex = 12;
			this.importSettingsButton.Text = "Import settings";
			this.importSettingsButton.UseVisualStyleBackColor = true;
			this.importSettingsButton.Click += new System.EventHandler(this.HandleImportSettingsButtonClick);
			// 
			// exportSettingsButton
			// 
			this.exportSettingsButton.Location = new System.Drawing.Point(311, 62);
			this.exportSettingsButton.Name = "exportSettingsButton";
			this.exportSettingsButton.Size = new System.Drawing.Size(110, 23);
			this.exportSettingsButton.TabIndex = 13;
			this.exportSettingsButton.Text = "Export settings";
			this.exportSettingsButton.UseVisualStyleBackColor = true;
			this.exportSettingsButton.Click += new System.EventHandler(this.HandleExportSettingsButtonClick);
			this.versionLink.Name = "versionLink";
			this.versionLink.Size = new System.Drawing.Size(0, 13);
			this.versionLink.TabIndex = 8;
			this.versionLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleVersionLinkClicked);
			// 
			// OptionsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.themePreview);
			this.Controls.Add(this.versionLink);
			this.Controls.Add(this.openThemeFolderButton);
			this.Controls.Add(this.importSettingsButton);
			this.Controls.Add(this.exportSettingsButton);
			this.Controls.Add(this.checkUpdatesCheckBox);
			this.Controls.Add(this.checkUpdatesLabel);
			this.Controls.Add(this.editTemplateButton);
			this.Controls.Add(this.themeTemplateComboBox);
			this.Controls.Add(this.themeTemplate);
			this.Controls.Add(this.autoSyncWin10ThemeLabel);
			this.Controls.Add(this.autoSyncWin10ThemeCheckBox);
			this.Controls.Add(this.hotKeyTextBox);
			this.Controls.Add(this.hotKeyLabel);
			this.Name = "OptionsPanel";
			this.Size = new System.Drawing.Size(645, 200);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.Windows.Forms.Button editTemplateButton;

		private System.Windows.Forms.LinkLabel versionLink;

		private System.Windows.Forms.Label checkUpdatesLabel;
		private System.Windows.Forms.CheckBox checkUpdatesCheckBox;

		private KeePassDarkMode.Options.ThemePreviewControl themePreview;
		private System.Windows.Forms.Button openThemeFolderButton;
		private System.Windows.Forms.Button importSettingsButton;
		private System.Windows.Forms.Button exportSettingsButton;

		private System.Windows.Forms.Label themeTemplate;
		private System.Windows.Forms.ComboBox themeTemplateComboBox;

		private System.Windows.Forms.CheckBox autoSyncWin10ThemeCheckBox;

		private System.Windows.Forms.Label autoSyncWin10ThemeLabel;

		private System.Windows.Forms.Label hotKeyLabel;
		private KeePass.UI.HotKeyControlEx hotKeyTextBox;

		#endregion
	}
}