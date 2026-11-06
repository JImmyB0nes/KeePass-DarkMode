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

		private readonly DarkModeOptions _options;

		public Win10ThemeMonitor(DarkModeOptions options)
		{
			_options = options;
		}

		public void Initialize()
		{
			if (WinUtil.IsAtLeastWindows10)
			{
				if (_options.AutoSyncWithWin10Theme)
				{
					_options.Enabled = IsDarkThemeEnabled();
				}
				_options.AutoSyncWithWin10ThemeChanged += HandleAutoSyncWithWin10ThemeChanged;
				SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;
			}
		}

		public void Terminate()
		{
			if (WinUtil.IsAtLeastWindows10)
			{
				_options.AutoSyncWithWin10ThemeChanged -= HandleAutoSyncWithWin10ThemeChanged;
				SystemEvents.UserPreferenceChanged -= HandleUserPreferenceChanged;
			}
		}
		
		private void HandleAutoSyncWithWin10ThemeChanged(bool value)
		{
			ApplyTheme();
		}

		private void HandleUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
		{
			ApplyTheme();
		}

		private void ApplyTheme()
		{
			if (_options.AutoSyncWithWin10Theme) 
				_options.Enabled = IsDarkThemeEnabled();
		}

		private bool IsDarkThemeEnabled()
		{
			try
			{
				var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
				if (key == null)
					return false;

				// Registry values are not guaranteed to be int - use GetValueOrDefault
				// so an unexpected type does not throw.
				var value = key.GetValue("AppsUseLightTheme", 1);
				return value is int && (int) value != 1;
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