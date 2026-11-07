using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KeePass.Util;
using KeePassDarkMode.Options;
using Microsoft.Win32;

namespace KeePassDarkMode
{
	internal class Win10ThemeMonitor
	{
		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

		private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
		private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

		private const string PersonalizeKeyPath =
			@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

		private readonly DarkModeOptions _options;
		private RegistryKey _personalizeKey;
		private Timer _debounceTimer;

		public Win10ThemeMonitor(DarkModeOptions options)
		{
			_options = options;
		}

		public void Initialize()
		{
			if (!WinUtil.IsAtLeastWindows10)
				return;

			if (_options.AutoSyncWithWin10Theme)
				SyncNow();

			_options.AutoSyncWithWin10ThemeChanged += HandleAutoSyncWithWin10ThemeChanged;
			SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;
			StartRegistryWatch();
		}

		public void Terminate()
		{
			if (!WinUtil.IsAtLeastWindows10)
				return;

			_options.AutoSyncWithWin10ThemeChanged -= HandleAutoSyncWithWin10ThemeChanged;
			SystemEvents.UserPreferenceChanged -= HandleUserPreferenceChanged;
			StopRegistryWatch();

			if (_debounceTimer != null)
			{
				_debounceTimer.Dispose();
				_debounceTimer = null;
			}
		}

		private void HandleAutoSyncWithWin10ThemeChanged(bool value)
		{
			// Toggling the option itself should follow the current Windows theme
			// immediately, not only on the next preference change.
			ScheduleSync();
		}

		private void HandleUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
		{
			// Fires for wallpaper, accent color, high contrast, ... - the
			// registry watcher and the debounced re-read keep it cheap.
			ScheduleSync();
		}

		private void HandleRegistryNotifyChanged(object sender, RegistryEventArgs e)
		{
			ScheduleSync();
		}

		// The registry write and the preference broadcast can race, and the
		// broadcast can arrive before AppsUseLightTheme is updated. Re-read
		// shortly after the event so the final value always wins.
		private void ScheduleSync()
		{
			if (_debounceTimer == null)
			{
				_debounceTimer = new Timer { Interval = 300 };
				_debounceTimer.Tick += delegate
				{
					_debounceTimer.Stop();
					SyncNow();
				};
			}

			_debounceTimer.Stop();
			_debounceTimer.Start();
		}

		private void SyncNow()
		{
			if (!_options.AutoSyncWithWin10Theme)
				return;

			var dark = IsDarkThemeEnabled();

			// Only touch the option when it actually differs - avoids config
			// churn and a full theme repaint on unrelated preference events.
			if (_options.Enabled != dark)
				_options.Enabled = dark;
		}

		private void StartRegistryWatch()
		{
			try
			{
				// A plain read-open already includes KEY_NOTIFY in its access
				// mask (.NET Framework: KEY_READ = Query|Enumerate|Notify), so
				// no write access is needed for the change notification.
				_personalizeKey = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath);
				if (_personalizeKey != null)
					_personalizeKey.NotifyChanged += HandleRegistryNotifyChanged;
			}
			catch (Exception)
			{
				// Not readable (restricted policy) - the broadcast path still works.
				_personalizeKey = null;
			}
		}

		private void StopRegistryWatch()
		{
			if (_personalizeKey == null)
				return;

			try
			{
				_personalizeKey.NotifyChanged -= HandleRegistryNotifyChanged;
				_personalizeKey.Close();
			}
			catch (Exception)
			{
				// ignored
			}

			_personalizeKey = null;
		}

		private static bool IsDarkThemeEnabled()
		{
			try
			{
				using (var key = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath))
				{
					if (key == null)
						return false;

					var value = key.GetValue("AppsUseLightTheme");
					if (value == null)
						return false;

					// DWORD is the normal type, but be tolerant about odd types.
					return Convert.ToInt32(value) != 1;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static void UseImmersiveDarkMode(Form form, bool enabled)
		{
			if (form.IsHandleCreated)
			{
				if (UseImmersiveDarkMode(form.Handle, enabled))
				{
					// Hack: I have found no other way to redraw title bar
					var borderStyle = form.FormBorderStyle;
					form.FormBorderStyle = FormBorderStyle.None;
					form.FormBorderStyle = borderStyle;
				}
			}
			else
			{
				form.HandleCreated += (o, args) => UseImmersiveDarkMode(form.Handle, enabled);
			}
		}

		private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
		{
			if (IsWindows10OrGreater(17763))
			{
				var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
				if (IsWindows10OrGreater(18985))
				{
					attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
				}

				int useImmersiveDarkMode = enabled ? 1 : 0;
				return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
			}

			return false;
		}

		private static bool IsWindows10OrGreater(int build)
		{
			if (!WinUtil.IsAtLeastWindows10)
				return false;

			try
			{
				var registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", false);
				if (registryKey != null)
				{
					uint result;
					if (uint.TryParse(registryKey.GetValue("CurrentBuildNumber", string.Empty).ToString(), out result))
						return result >= build;
				}
			}
			catch (Exception)
			{
				// ignored
			}

			return false;
		}
	}
}
