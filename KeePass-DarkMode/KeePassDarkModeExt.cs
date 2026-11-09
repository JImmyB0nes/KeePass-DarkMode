using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using KeePass;
using KeePass.Ecas;
using KeePass.Forms;
using KeePass.Plugins;
using KeePassLib;
using KeePassLib.Utility;
using KeePassDarkMode.Decorators;
using KeePassDarkMode.Options;
using KeePassDarkMode.Properties;

namespace KeePassDarkMode
{
	public sealed class KeePassDarkModeExt : Plugin
	{
		// Copied from EcasEventIDs.AppInitPost instead of using reflection because probably it will never change
		private static readonly PwUuid AppInitPost = new PwUuid(new byte[] {
			0xD4, 0xCE, 0xCD, 0xB5, 0x4B, 0x98, 0x4F, 0xF2,
			0xA6, 0xA9, 0xE2, 0x55, 0x26, 0x1E, 0xC8, 0xE8
		});

		private ControlVisitor _controlVisitor;
		private DarkMode _theme;
		private IPluginHost _host;
		private ToolStripMenuItem _menuItem;
		private Version _pendingUpdate;
		private DarkModeOptions _options;
		private Win10ThemeMonitor _win10ThemeMonitor;
		private WindowIconDecorator _windowIcon;
		private NativeDialogWatcher _dialogWatcher;
		private readonly Dictionary<Form, FirstFrameDecorator> _firstFrames =
			new Dictionary<Form, FirstFrameDecorator>();
		private bool _initialized;

		public override bool Initialize(IPluginHost host)
		{
			if (host == null)
				return false;

			_host = host;

			_options = new DarkModeOptions(host);
			_controlVisitor = new ControlVisitor(HandleControlVisit);
			_theme = new DarkMode(_options);

			// Branding icon for the running main window (never the EXE/shortcuts).
			_windowIcon = new WindowIconDecorator(host.MainWindow);

			// Dark title bars for native dialogs (MessageBox, TaskDialog, ...).
			_dialogWatcher = new NativeDialogWatcher();

			_win10ThemeMonitor = new Win10ThemeMonitor(_options);
			_win10ThemeMonitor.Initialize();

			if (Program.TriggerSystem.Enabled)
			{
				// It's better to enable theme as late as possible, but not too late
				Program.TriggerSystem.RaisingEvent += HandleTriggerSystemRaisingEvent;
			}
			else
			{
				InitializeTheme();
			}
			
			AttachApplicationOpenFormsAddedHandler();

			// Silent background check - surfaces as a badge on the menu item.
			UpdateChecker.UpdateAvailable += HandleUpdateAvailable;
			UpdateChecker.Check(_options);

			return true;
		}

		private void AttachApplicationOpenFormsAddedHandler()
		{
			var customArrayList = new FormsArrayList();
			customArrayList.AddRange(Application.OpenForms);
			customArrayList.Added += HandleOpenFormsAdded;
			
			var listField = typeof(ReadOnlyCollectionBase).GetField("list", BindingFlags.Instance | BindingFlags.NonPublic);
			if (listField != null)
				listField.SetValue(Application.OpenForms, customArrayList);
		}

		private void HandleOpenFormsAdded(object sender, FormAddedEventArgs args)
		{
			if (!_initialized)
				InitializeTheme();

			// Cover the form with a transparent layer until the theme paint
			// finished - removes the light first-frame flash on new dialogs.
			PrepareFirstFrame(args.Form);

			if (_theme.Enabled)
				_controlVisitor.Visit(args.Form);
			
			Win10ThemeMonitor.UseImmersiveDarkMode(args.Form, _theme.Enabled);
			
			var optionsForm = args.Form as OptionsForm;
			if (optionsForm != null)
			{
				optionsForm.Shown -= HandleOptionsFormShown;
				optionsForm.Shown += HandleOptionsFormShown;
			}

			var editStringForm = args.Form as EditStringForm;
			if (editStringForm != null)
			{
				editStringForm.Load -= HandleEditStringFormLoad;
				editStringForm.Load += HandleEditStringFormLoad;
			}			
			
			var pwEntryForm = args.Form as PwEntryForm;
			if (pwEntryForm != null)
			{
				pwEntryForm.Load -= HandlePwEntryFormFormLoad;
				pwEntryForm.Load += HandlePwEntryFormFormLoad;
			}
		}

		private void HandlePwEntryFormFormLoad(object sender, EventArgs e)
		{
			PwGeneratorMenuDecorator.TryFindAndDecorate(sender, _theme);
		}

		private void HandleEditStringFormLoad(object sender, EventArgs e)
		{
			PwGeneratorMenuDecorator.TryFindAndDecorate(sender, _theme);
		}

		private void PrepareFirstFrame(Form form)
		{
			if (!_theme.Enabled || _firstFrames.ContainsKey(form))
				return;

			// All dialogs: the layered/composited first-frame trick is removed
			// again on Shown, so it is cheap enough for every KeePass form.
			var name = form.GetType().FullName;
			if (!name.StartsWith("KeePass.Forms."))
				return;

			if (FirstFrameDecorator.IsNativeVisible(form))
				return;

			var frame = new FirstFrameDecorator(form);
			_firstFrames.Add(form, frame);
			form.Disposed += delegate
			{
				frame.Dispose();
				_firstFrames.Remove(form);
			};
		}

		private void HandleTriggerSystemRaisingEvent(object sender, EcasRaisingEventArgs e)
		{
			if (e.Event.Type.Equals(AppInitPost))
			{
				InitializeTheme();

				Program.TriggerSystem.RaisingEvent -= HandleTriggerSystemRaisingEvent;
			}
		}

		private void InitializeTheme()
		{
			if (_initialized) return; //Not required to fix issue #137
			_theme.Enabled = _options.Enabled;
			if (_windowIcon != null)
				_windowIcon.Apply(_theme.Enabled);
			if (_dialogWatcher != null)
				_dialogWatcher.Enable(_theme.Enabled);
			// A splash or restored dialog can enter OpenForms before MainForm is
			// assigned - those forms get themed via HandleOpenFormsAdded instead.
			if (_theme.Enabled && Program.MainForm != null)
			{
				if (Program.MainForm.InvokeRequired)
				{
					Program.MainForm.BeginInvoke(new Action(() => { ApplyThemeInOpenForms(); }));
				}
				else ApplyThemeInOpenForms();
			}

			_options.EnabledChanged += enable =>
			{
				_theme.Enabled = enable;
				// Keep the menu check mark in sync when the toggle comes from
				// somewhere other than the menu click (Win10 auto-sync, hotkey).
				if (_menuItem != null && _menuItem.Checked != enable)
					_menuItem.Checked = enable;
				if (_windowIcon != null)
					_windowIcon.Apply(enable);
				if (_dialogWatcher != null)
					_dialogWatcher.Enable(enable);
				ApplyThemeInOpenForms();
			};
			
			_options.TemplateChanged += templatePath =>
			{
				if (_theme.Enabled)
				{
					// Reload instead of disable/enable toggle - avoids double repaint.
					_theme.Reload();

					// GetMenuItem may not have been called yet (e.g. template restored
					// from config before KeePass queries the menu) - guard against null.
					var templateName = TemplateReader.GetTemplateName(templatePath);
					if (_menuItem != null && !string.IsNullOrEmpty(templateName))
						_menuItem.Text = templateName;

					ApplyThemeInOpenForms();
				}
			};

			// Windows theme flipped or the accent color changed - re-resolve
			// the effective template.
			_options.ThemeModeChanged += HandleThemeModeChanged;
			_options.AccentFromWindowsChanged += accent => HandleThemeModeChanged();
			
			_initialized = true;
		}

		public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
		{
			if (t == PluginMenuType.Main)
			{
				_menuItem = new ToolStripMenuItem(_theme.Name);
				_menuItem.CheckOnClick = true;
				_menuItem.Checked = _options.Enabled;
				_menuItem.ShortcutKeys = _options.HotKey;
				_menuItem.Click += HandleToggleDarkModeMenuItemClick;
				_options.HotKeyChanged += keys => _menuItem.ShortcutKeys = keys;
				if (_pendingUpdate != null)
					_menuItem.Text = MenuBaseText() + string.Format(" - Update {0} available!", _pendingUpdate.ToString(3));
				return _menuItem;
			}

			return base.GetMenuItem(t);
		}

		private void HandleUpdateAvailable(Version latest)
		{
			_pendingUpdate = latest;
			if (_menuItem == null)
				return;

			// Menu items can only be updated from the UI thread.
			if (Program.MainForm != null && Program.MainForm.InvokeRequired)
			{
				Program.MainForm.BeginInvoke(new Action(UpdateMenuBadge));
				return;
			}

			UpdateMenuBadge();
		}

		private void UpdateMenuBadge()
		{
			if (_menuItem != null && _pendingUpdate != null)
				_menuItem.Text = MenuBaseText() + string.Format(" - Update {0} available!", _pendingUpdate.ToString(3));
		}

		private string MenuBaseText()
		{
			return _theme != null ? _theme.Name : "Dark Theme";
		}

		private void HandleToggleDarkModeMenuItemClick(object sender, EventArgs eventArgs)
		{
			_options.Enabled = !_options.Enabled;
		}

		private void HandleThemeModeChanged()
		{
			if (!_theme.Enabled)
				return;

			_theme.Reload();

			if (_menuItem != null)
				_menuItem.Text = _theme.Name;

			// Title bars must flip with the mode (immersive dark attribute).
			if (_dialogWatcher != null)
				_dialogWatcher.Enable(_theme.Enabled);
			foreach (Form openForm in Application.OpenForms)
				Win10ThemeMonitor.UseImmersiveDarkMode(openForm, _theme.Enabled);

			ApplyThemeInOpenForms();
		}

		private void ApplyThemeInOpenForms()
		{
			foreach (Form openForm in Application.OpenForms)
			{
				Win10ThemeMonitor.UseImmersiveDarkMode(openForm, _theme.Enabled);
				_controlVisitor.Visit(openForm);
			}
			if (Program.MainForm != null)
				Program.MainForm.RefreshEntriesList();
		}

		private void HandleControlVisit(Control control)
		{
			_theme.Apply(control);
		}

		private void HandleOptionsFormShown(object sender, EventArgs e)
		{
			var optionsForm = (OptionsForm) sender;
			OptionsPanel.Create(optionsForm, _options);
		}

		public override void Terminate()
		{
			if (_win10ThemeMonitor != null)
				_win10ThemeMonitor.Terminate();

			foreach (var frame in _firstFrames.Values)
				frame.Dispose();
			_firstFrames.Clear();

			if (_windowIcon != null)
			{
				_windowIcon.Apply(false);
				_windowIcon.Dispose();
				_windowIcon = null;
			}

			if (_dialogWatcher != null)
			{
				_dialogWatcher.Dispose();
				_dialogWatcher = null;
			}

			if (Program.TriggerSystem != null)
				Program.TriggerSystem.RaisingEvent -= HandleTriggerSystemRaisingEvent;

			UpdateChecker.UpdateAvailable -= HandleUpdateAvailable;

			if (_theme != null)
			{
				_theme.Enabled = false;
				_theme.CleanupCenteredSearch();
				_theme.CleanupShieldButtons();
			}

			// Restore the original OpenForms list that we replaced in AttachApplicationOpenFormsHandler.
			// Application.OpenForms is a sealed FormCollection, so our FormsArrayList lives in its
			// private 'list' field and must be read via reflection (same as when installing it).
			var listField = typeof(ReadOnlyCollectionBase).GetField("list", BindingFlags.Instance | BindingFlags.NonPublic);
			var customArrayList = listField != null
				? listField.GetValue(Application.OpenForms) as FormsArrayList
				: null;
			if (customArrayList != null)
			{
				customArrayList.Added -= HandleOpenFormsAdded;

				var plainList = new ArrayList();
				foreach (Form form in customArrayList)
					plainList.Add(form);

				listField.SetValue(Application.OpenForms, plainList);
			}
		}

		public override string UpdateUrl
		{
			// KeePass's Update Check parses a plain text file "Name:Version" -
			// a releases page (HTML) would only yield "Unknown".
			get { return "https://raw.githubusercontent.com/JImmyB0nes/KeePass-DarkMode/master/update.txt"; }
		}

		public override Image SmallIcon
		{
			get { return GfxUtil.ScaleImage(Resource.PluginIcon, 16, 16); }
		}
	}
}