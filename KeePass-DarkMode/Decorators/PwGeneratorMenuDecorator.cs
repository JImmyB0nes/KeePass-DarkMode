using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using KeePass.UI;

namespace KeePassDarkMode.Decorators
{
	public static class PwGeneratorMenuDecorator
	{
		internal static void TryFindAndDecorate(object sender, DarkMode darkMode)
		{
			if (!darkMode.Enabled)
				return;

			try
			{
				FindAndDecorate(sender, darkMode);
			}
			catch (Exception)
			{
				// Ignore because users should be able to use KeePass anyways.
			}
		}

		private static void FindAndDecorate(object sender, DarkMode darkMode)
		{
			var fields = sender.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			var pwGeneratorMenuField = fields.FirstOrDefault(x => x.FieldType.Name == "PwGeneratorMenu");
			if (pwGeneratorMenuField == null)
				return;

			var pwGeneratorMenuType = pwGeneratorMenuField.FieldType;
			var pwGeneratorMenu = pwGeneratorMenuField.GetValue(sender);

			var btnHostField = pwGeneratorMenuType.GetField("m_btnHost", BindingFlags.Instance | BindingFlags.NonPublic);
			var btnHost = (Button) btnHostField.GetValue(pwGeneratorMenu);

			var onHostBtnClickMethod =
				pwGeneratorMenuType.GetMethod("OnHostBtnClick", BindingFlags.Instance | BindingFlags.NonPublic);
			var onHostBtnClick = Delegate.CreateDelegate(typeof(EventHandler), pwGeneratorMenu, onHostBtnClickMethod.Name);

			var eventInstance = typeof(Control)
				.GetField("EventClick", BindingFlags.Static | BindingFlags.NonPublic).GetValue(btnHost);

			var eventHandlerList = (EventHandlerList) typeof(Button)
				.GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(btnHost, null);

			eventHandlerList.RemoveHandler(eventInstance, onHostBtnClick);

			var constructContextMenuMethod =
				pwGeneratorMenuType.GetMethod("ConstructContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);

			btnHost.Click += (o, args) =>
			{
				constructContextMenuMethod.Invoke(pwGeneratorMenu, null);
				var contextMenuField = pwGeneratorMenuType.GetField("m_ctx", BindingFlags.Instance | BindingFlags.NonPublic);
				var contextMenu = (CustomContextMenuStripEx) contextMenuField.GetValue(pwGeneratorMenu);

				darkMode.Apply(contextMenu);

				contextMenu.ShowEx(btnHost);
			};
		}
	}
}