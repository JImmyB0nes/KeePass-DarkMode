using System;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using KeePassDarkMode.Options;

namespace KeePassDarkMode
{
	// Background update check against the GitHub releases API. Runs once per
	// plugin session, throttled via LastUpdateCheck, and raises UpdateAvailable
	// so the menu item can show a badge without opening the options dialog.
	internal static class UpdateChecker
	{
		private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);
		private const string ReleaseApiUrl = "https://api.github.com/repos/JImmyB0nes/KeePass-DarkMode/releases/latest";

		private static bool _running;

		public static event Action<Version> UpdateAvailable;

		public static Version CurrentVersion
		{
			get
			{
				var version = Assembly.GetExecutingAssembly().GetName().Version;
				return new Version(version.Major, version.Minor, version.Build);
			}
		}

		public static void Check(DarkModeOptions options)
		{
			if (!options.CheckUpdates || _running)
				return;

			// Fresh cached result: report it without hitting the network again.
			if (DateTime.UtcNow - options.LastUpdateCheck < CheckInterval)
			{
				RaiseIfNewer(ParseCached(options));
				return;
			}

			_running = true;
			var worker = new BackgroundWorker();
			worker.DoWork += (s, args) => args.Result = QueryLatestVersion();
			worker.RunWorkerCompleted += (s, args) =>
			{
				_running = false;
				var latest = args.Error == null ? args.Result as Version : null;
				if (latest == null)
					return;

				options.LastUpdateCheck = DateTime.UtcNow;
				options.LatestVersion = latest.ToString(3);
				RaiseIfNewer(latest);
			};
			worker.RunWorkerAsync();
		}

		private static Version ParseCached(DarkModeOptions options)
		{
			Version cached;
			return Version.TryParse(options.LatestVersion, out cached) ? cached : null;
		}

		private static void RaiseIfNewer(Version latest)
		{
			if (latest != null && latest > CurrentVersion && UpdateAvailable != null)
				UpdateAvailable(latest);
		}

		public static Version QueryLatestVersion()
		{
			try
			{
				// Add TLS 1.2 for the GitHub handshake without replacing the
				// protocols KeePass or other plugins may rely on.
				ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
				var request = (HttpWebRequest)WebRequest.Create(ReleaseApiUrl);
				request.UserAgent = "KeePass-DarkMode";
				request.Timeout = 5000;
				using (var response = (HttpWebResponse)request.GetResponse())
				using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
				{
					var json = reader.ReadToEnd();
					var match = Regex.Match(json, "\"tag_name\"\\s*:\\s*\"v?(\\d+\\.\\d+\\.\\d+)\"");
					if (match.Success)
						return Version.Parse(match.Groups[1].Value);
				}
			}
			catch (Exception)
			{
				// No internet, proxy, rate limit - just stay quiet.
			}

			return null;
		}
	}
}
