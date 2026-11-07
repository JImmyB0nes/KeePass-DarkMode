using System;
using System.Windows.Forms;
using KeePass.Plugins;

namespace KeePassDarkMode.Options
{
	public class DarkModeOptions
	{
		private const string TemplateOption = "KeePass-DarkMode.Template";
		private const string EnabledOption = "KeePass-DarkMode.Enabled";
		private const string HotKeyOption = "KeePass-DarkMode.HotKey";
		private const string AutoSyncWithWin10ThemeOption = "KeePass-DarkMode.AutoSyncWithWin10Theme";
		private const string AutoLightThemeOption = "KeePass-DarkMode.AutoLightTheme";
		private const string LightTemplateOption = "KeePass-DarkMode.LightTemplate";
		private const string AccentFromWindowsOption = "KeePass-DarkMode.AccentFromWindows";
		private const string CheckUpdatesOption = "KeePass-DarkMode.CheckUpdates";
		private const string LastUpdateCheckOption = "KeePass-DarkMode.LastUpdateCheck";
		private const string LatestVersionOption = "KeePass-DarkMode.LatestVersion";
		
		private readonly IPluginHost _pluginHost;
		private bool _enabled;
		private Keys _hotKey;
		private bool _autoSyncWithWin10Theme;
		private bool _autoLightTheme;
		private string _lightTemplate;
		private bool _accentFromWindows;
		private bool _checkUpdates;
		private string _template;

		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;
				_pluginHost.CustomConfig.SetBool(EnabledOption, value);
				if (EnabledChanged != null)
					EnabledChanged.Invoke(value);
			}
		}

		public Keys HotKey
		{
			get { return _hotKey; }
			set
			{
				_hotKey = value;
				_pluginHost.CustomConfig.SetString(HotKeyOption, value.ToString());
				if (HotKeyChanged != null)
					HotKeyChanged.Invoke(value);
			}
		}

		public bool AutoSyncWithWin10Theme
		{
			get { return _autoSyncWithWin10Theme; }
			set
			{
				_autoSyncWithWin10Theme = value;
				_pluginHost.CustomConfig.SetBool(AutoSyncWithWin10ThemeOption, value);
				if (AutoSyncWithWin10ThemeChanged != null)
					AutoSyncWithWin10ThemeChanged.Invoke(value);
			}
		}

		/// <summary>
		/// In auto-sync mode, apply the light template when Windows is in
		/// light mode instead of switching the plugin off.
		/// </summary>
		public bool AutoLightTheme
		{
			get { return _autoLightTheme; }
			set
			{
				_autoLightTheme = value;
				_pluginHost.CustomConfig.SetBool(AutoLightThemeOption, value);
				if (AutoLightThemeChanged != null)
					AutoLightThemeChanged.Invoke(value);
			}
		}

		/// <summary>
		/// Template path used when Windows runs in light mode.
		/// </summary>
		public string LightTemplate
		{
			get { return _lightTemplate; }
			set
			{
				if (_lightTemplate == value)
					return;

				_lightTemplate = value;
				_pluginHost.CustomConfig.SetString(LightTemplateOption, value);
				if (LightTemplateChanged != null)
					LightTemplateChanged.Invoke(value);
			}
		}

		/// <summary>
		/// Override the theme's highlight colors with the current Windows accent color.
		/// </summary>
		public bool AccentFromWindows
		{
			get { return _accentFromWindows; }
			set
			{
				_accentFromWindows = value;
				_pluginHost.CustomConfig.SetBool(AccentFromWindowsOption, value);
				if (AccentFromWindowsChanged != null)
					AccentFromWindowsChanged.Invoke(value);
			}
		}
		
		public string Template
		{
			get { return _template; }
			set
			{
				if (_template == value)
					return;
				
				_template = value;
				_pluginHost.CustomConfig.SetString(TemplateOption, value);
				if (TemplateChanged != null)
					TemplateChanged.Invoke(value);
			}
		}

		/// <summary>
		/// Whether the background update check against GitHub may run.
		/// </summary>
		public bool CheckUpdates
		{
			get { return _checkUpdates; }
			set
			{
				_checkUpdates = value;
				_pluginHost.CustomConfig.SetBool(CheckUpdatesOption, value);
			}
		}

		/// <summary>
		/// UTC time of the last successful update check (DateTime.MinValue if never).
		/// </summary>
		public DateTime LastUpdateCheck
		{
			get
			{
				long ticks;
				var raw = _pluginHost.CustomConfig.GetString(LastUpdateCheckOption, string.Empty);
				return long.TryParse(raw, out ticks) && ticks > 0 ? new DateTime(ticks) : DateTime.MinValue;
			}
			set { _pluginHost.CustomConfig.SetString(LastUpdateCheckOption, value.Ticks.ToString()); }
		}

		/// <summary>
		/// Highest version seen during the last update check (empty if none).
		/// </summary>
		public string LatestVersion
		{
			get { return _pluginHost.CustomConfig.GetString(LatestVersionOption, string.Empty); }
			set { _pluginHost.CustomConfig.SetString(LatestVersionOption, value ?? string.Empty); }
		}

		private const string ConfigFileHeader = "KeePass-DarkMode-Config v1";

		/// <summary>
		/// Serializes all plugin settings for sharing between machines.
		/// </summary>
		public string ExportSettings()
		{
			return string.Join(Environment.NewLine, new[]
			{
				ConfigFileHeader,
				TemplateOption + "=" + _template,
				EnabledOption + "=" + _enabled,
				HotKeyOption + "=" + _hotKey,
				AutoSyncWithWin10ThemeOption + "=" + _autoSyncWithWin10Theme,
				AutoLightThemeOption + "=" + _autoLightTheme,
				LightTemplateOption + "=" + _lightTemplate,
				AccentFromWindowsOption + "=" + _accentFromWindows,
				CheckUpdatesOption + "=" + _checkUpdates
			});
		}

		/// <summary>
		/// Applies settings from ExportSettings output. Fires the change events
		/// so the running theme follows immediately.
		/// </summary>
		public void ImportSettings(string data)
		{
			if (string.IsNullOrEmpty(data) ||
				!data.StartsWith(ConfigFileHeader, StringComparison.Ordinal))
				throw new FormatException("Not a KeePass-DarkMode settings file.");

			foreach (var rawLine in data.Split(new[] { "\r\n", "\n" },
				StringSplitOptions.RemoveEmptyEntries))
			{
				var line = rawLine.Trim();
				var separator = line.IndexOf('=');
				if (separator <= 0)
					continue;

				var key = line.Substring(0, separator).Trim();
				var value = line.Substring(separator + 1).Trim();
				switch (key)
				{
					case TemplateOption:
						Template = value;
						break;
					case EnabledOption:
						Enabled = bool.Parse(value);
						break;
					case HotKeyOption:
						HotKey = (Keys) Enum.Parse(typeof(Keys), value);
						break;
					case AutoSyncWithWin10ThemeOption:
						AutoSyncWithWin10Theme = bool.Parse(value);
						break;
					case AutoLightThemeOption:
						AutoLightTheme = bool.Parse(value);
						break;
					case LightTemplateOption:
						LightTemplate = value;
						break;
					case AccentFromWindowsOption:
						AccentFromWindows = bool.Parse(value);
						break;
					case CheckUpdatesOption:
						CheckUpdates = bool.Parse(value);
						break;
				}
			}
		}

		public event Action<bool> EnabledChanged;
		public event Action<Keys> HotKeyChanged;
		public event Action<bool> AutoSyncWithWin10ThemeChanged;
		public event Action<bool> AutoLightThemeChanged;
		public event Action<string> LightTemplateChanged;
		public event Action<bool> AccentFromWindowsChanged;
		public event Action<string> TemplateChanged;

		/// <summary>
		/// Raised when the effective theme mode changed (dark/light switch by
		/// Windows, accent color change) although no option value changed.
		/// </summary>
		public event Action ThemeModeChanged;

		public void NotifyThemeModeChanged()
		{
			if (ThemeModeChanged != null)
				ThemeModeChanged.Invoke();
		}

		public DarkModeOptions(IPluginHost pluginHost)
		{
			_pluginHost = pluginHost;
			
			_enabled = pluginHost.CustomConfig.GetBool(EnabledOption, false);
			var hotKey = pluginHost.CustomConfig.GetString(HotKeyOption, "T, Control");
			try
			{
				_hotKey = (Keys) Enum.Parse(typeof(Keys), hotKey);
			}
			catch (Exception)
			{
				// Corrupted config value - fall back to the default hot key
				// instead of failing the whole plugin load.
				_hotKey = Keys.Control | Keys.T;
			}
			_autoSyncWithWin10Theme = pluginHost.CustomConfig.GetBool(AutoSyncWithWin10ThemeOption, true);
			_autoLightTheme = pluginHost.CustomConfig.GetBool(AutoLightThemeOption, true);
			_accentFromWindows = pluginHost.CustomConfig.GetBool(AccentFromWindowsOption, false);
			_checkUpdates = pluginHost.CustomConfig.GetBool(CheckUpdatesOption, true);
			_template = pluginHost.CustomConfig.GetString(TemplateOption, TemplateReader.DefaultTemplatePath);
			_lightTemplate = pluginHost.CustomConfig.GetString(LightTemplateOption, TemplateReader.DefaultLightTemplatePath);
		}
	}
}