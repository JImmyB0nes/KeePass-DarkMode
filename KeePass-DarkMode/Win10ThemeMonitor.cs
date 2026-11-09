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
		private Timer _debounceTimer;
		private bool _initializedDarkState;
		private Timer _watchdogTimer;
		private bool _lastKnownDark;

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

			// Slow watchdog: catches theme switches that the UserPreferenceChanged
			// broadcast may miss (remote sessions, some Windows 11 paths). A single
			// cached registry read every few seconds is negligible.
			_watchdogTimer = new Timer { Interval = 5000 };
			_watchdogTimer.Tick += delegate { SyncIfChanged(); };
			_watchdogTimer.Start();
		}

		public void Terminate()
		{
			if (!WinUtil.IsAtLeastWindows10)
				return;

			_options.AutoSyncWithWin10ThemeChanged -= HandleAutoSyncWithWin10ThemeChanged;
			SystemEvents.UserPreferenceChanged -= HandleUserPreferenceChanged;

			if (_debounceTimer != null)
			{
				_debounceTimer.Dispose();
				_debounceTimer = null;
			}

			if (_watchdogTimer != null)
			{
				_watchdogTimer.Dispose();
				_watchdogTimer = null;
			}
		}

		private void HandleAutoSyncWithWin10ThemeChanged(bool value)
		{
			// Toggling the option itself should follow the current Windows theme
			// immediately, not only on the next preference change. SyncIfChanged
			// would do nothing here (the Windows value did not change), so
			// sync unconditionally.
			if (value)
				SyncNow();
		}

		private void HandleUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
		{
			// Fires for wallpaper, accent color, high contrast, ... - the sync
			// only acts when the dark/light value really changed.
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
					SyncIfChanged();
				};
			}

			_debounceTimer.Stop();
			_debounceTimer.Start();
		}

		// Acts only when the Windows setting itself changed. A manual Ctrl+T
		// toggle must stay valid until the next real Windows theme switch,
		// so the watchdog must not "correct" Enabled back on every poll.
		// Note: on KeePass start (SyncNow) the Windows theme wins again.
		private void SyncIfChanged()
		{
			var dark = IsDarkThemeEnabled();
			var changed = dark != _lastKnownDark || !_initializedDarkState;
			_lastKnownDark = dark;
			_initializedDarkState = true;

			if (!_options.AutoSyncWithWin10Theme)
				return;

			// Dark Windows mode = plugin on, light mode = plugin off.
			// A manual Ctrl+T toggle stays valid until the next real Windows
			// theme switch or KeePass restart - the watchdog only acts when the
			// value really changed.
			if (!changed)
				return;
			if (_options.Enabled != dark)
				_options.Enabled = dark;
		}

		private void SyncNow()
		{
			_initializedDarkState = true;
			_lastKnownDark = IsDarkThemeEnabled();

			if (!_options.AutoSyncWithWin10Theme)
				return;

			if (_options.Enabled != _lastKnownDark)
				_options.Enabled = _lastKnownDark;
		}

		public static bool IsDarkThemeEnabled()
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

		// UI thread only - no locking needed.
		private static readonly System.Collections.Generic.Dictionary<Form, bool> PendingDarkMode =
			new System.Collections.Generic.Dictionary<Form, bool>();

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
				// One self-removing handler per form: repeated calls before the
				// handle exists only update the requested state (last one wins).
				var alreadyHooked = PendingDarkMode.ContainsKey(form);
				PendingDarkMode[form] = enabled;
				if (alreadyHooked)
					return;

				EventHandler hook = null;
				hook = (s, e) =>
				{
					form.HandleCreated -= hook;
					bool pending;
					if (!PendingDarkMode.TryGetValue(form, out pending))
						return;
					PendingDarkMode.Remove(form);
					UseImmersiveDarkMode(form.Handle, pending);
				};
				form.HandleCreated += hook;
				form.Disposed += (s, e) => PendingDarkMode.Remove(form);
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
				using (var registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", false))
				{
					uint result;
					if (registryKey != null &&
					    uint.TryParse(registryKey.GetValue("CurrentBuildNumber", string.Empty).ToString(), out result))
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
