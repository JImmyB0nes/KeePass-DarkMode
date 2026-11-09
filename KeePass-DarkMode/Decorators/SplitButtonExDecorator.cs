using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using KeePass.UI;
using KeePassDarkMode.Theme;

namespace KeePassDarkMode.Decorators
{
	internal class SplitButtonExDecorator : Control
	{
		const string EventClick = "EventClick";

		private ITheme _theme;
		private bool _enabled;
		
		private readonly SplitButtonEx _button;
		private readonly ContentAlignment _imageAlign;
		private readonly Image _image;
		private readonly EventHandler _clickHandler;
		// Reused across EnableTheme calls - assigning a fresh bitmap every
		// reload would leak one GDI object per theme change.
		private Image _arrowImage;
		private Color _arrowForeColor;
		// Tracks whether our click routing is installed - EnableTheme runs
		// several times per dialog, every switch must happen exactly once.
		private bool _themedClickInstalled;

		public SplitButtonExDecorator(SplitButtonEx button, ITheme theme)
		{
			_button = button;
			_theme = theme;

			_imageAlign = button.ImageAlign;
			_image = button.Image;
			_clickHandler = (EventHandler) GetClickEvent(_button);

			button.Controls.Add(this);
		}

		private static void RemoveClickEvent(SplitButtonEx button)
		{
			try
			{
				var eventInstance = typeof(Control)
					.GetField(EventClick, BindingFlags.Static | BindingFlags.NonPublic).GetValue(button);
				
				var list = (EventHandlerList) typeof(SplitButtonEx)
					.GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(button, null);
				
				list.RemoveHandler(eventInstance, list[eventInstance]);
			}
			catch (NullReferenceException)
			{
			}
		}
		
		private static object GetClickEvent(Component button)
		{
			try
			{
				var privateBinding = BindingFlags.Instance | BindingFlags.NonPublic;
				var key = typeof(Control)
					.GetField(EventClick, BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
				
				var events = typeof(Component).GetField("events", privateBinding).GetValue(button);
				var listEntry = typeof(EventHandlerList).GetMethod("Find", privateBinding).Invoke(events, new [] {key});
				var handler = listEntry.GetType().GetField("handler", privateBinding).GetValue(listEntry);
				return handler;
			}
			catch (ArgumentNullException)
			{
				return null;
			}
			catch (NullReferenceException)
			{
				return null;
			}
		}

		private void HandleButtonMouseClick(object sender, MouseEventArgs e)
		{
			var rect = new Rectangle(_button.Width - 20, 0, 20, _button.Height);
			if (rect.Contains(e.Location))
			{
				_button.SplitDropDownMenu.ShowEx(_button);
				// Kept as a safety net: re-show only if the menu is not visible.
				if (!_button.SplitDropDownMenu.Visible)
					_button.SplitDropDownMenu.ShowEx(_button);
			}
			else
			{
				if (_clickHandler != null)
					_clickHandler(this, e);
			}
		}

		private Image GetButtonDownArrow()
		{
			// Cached: same bitmap is reused for every theme reload.
			if (_arrowImage != null && _arrowForeColor == ForeColor)
				return _arrowImage;

			if (_arrowImage != null)
			{
				_arrowImage.Dispose();
				_arrowImage = null;
			}

			var bitmap = new Bitmap(16, 16);
			using (var g = Graphics.FromImage(bitmap))
			using (var brush = new SolidBrush(ForeColor))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.FillPolygon(brush, new[] {new Point(4, 6), new Point(12, 6), new Point(8, 10)});
			}

			_arrowImage = bitmap;
			_arrowForeColor = ForeColor;
			return _arrowImage;
		}

		public void EnableTheme(bool enabled, ITheme theme)
		{
			_enabled = enabled;
			_theme = theme;

			Apply(_button);
		}

		private void Apply(SplitButtonEx button)
		{
			if (_theme.Button.FlatStyle == FlatStyle.System)
				return;
			
			if (_enabled)
			{
				button.ImageAlign = ContentAlignment.MiddleRight;
				button.Image = GetButtonDownArrow();
				if (!_themedClickInstalled)
				{
					button.MouseClick -= HandleButtonMouseClick;
					button.MouseClick += HandleButtonMouseClick;
					RemoveClickEvent(_button);
					_themedClickInstalled = true;
				}
			}
			else
			{
				button.ImageAlign = _imageAlign;
				button.Image = _image;
				if (_themedClickInstalled)
				{
					button.MouseClick -= HandleButtonMouseClick;
					if (_clickHandler != null)
					{
						button.Click -= _clickHandler;
						button.Click += _clickHandler;
					}
					_themedClickInstalled = false;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _arrowImage != null)
			{
				_arrowImage.Dispose();
				_arrowImage = null;
			}

			base.Dispose(disposing);
		}
	}
}