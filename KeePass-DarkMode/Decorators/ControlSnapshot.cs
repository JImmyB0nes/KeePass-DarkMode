using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace KeePassDarkMode.Decorators
{
	static class ControlSnapshot
	{
		public static void Make(Control control)
		{
			var disabledControlName = control.Name + "Snapshot";

			var parent = control.Parent;
			if (parent == null)
				return;

			if (!parent.Enabled)
			{
				if (parent.Controls.ContainsKey(disabledControlName))
					return;

				// Can't use control.DrawToBitmap because:
				// RichTextBox - it does not draw text
				// ListView - it does not draw correct header color
				var screenCopy = TryCopyFromScreen(control);
				if (screenCopy == null)
					return;

				var controlSnapshot = new PictureBox();
				controlSnapshot.Name = disabledControlName;
				controlSnapshot.Size = control.Size;
				controlSnapshot.Location = control.Location;
				controlSnapshot.Image = screenCopy;
				parent.Controls.Add(controlSnapshot);
				parent.Controls.SetChildIndex(controlSnapshot, 0);
			}
			else
			{
				// RemoveByKey would leak the PictureBox and its screen bitmap.
				var index = parent.Controls.IndexOfKey(disabledControlName);
				if (index < 0)
					return;

				var snapshot = parent.Controls[index];
				parent.Controls.RemoveAt(index);
				var pictureBox = snapshot as PictureBox;
				if (pictureBox != null && pictureBox.Image != null)
				{
					var image = pictureBox.Image;
					pictureBox.Image = null;
					image.Dispose();
				}
				snapshot.Dispose();
			}
		}

		private static Bitmap TryCopyFromScreen(Control control)
		{
			try
			{
				return CopyFromScreen(control);
			}
			catch (ArgumentException)
			{
				return null;
			}
			catch (InvalidOperationException)
			{
				return null;
			}
			catch (Win32Exception)
			{
				return null;
			}
		}

		private static Bitmap CopyFromScreen(Control control)
		{
			control.Update(); // Ensure control is fully painted
			var bitmap = new Bitmap(control.Bounds.Width, control.Bounds.Height);
			using (var g = Graphics.FromImage(bitmap))
			{
				// ListView.PointToScreen returns result without border
				var upperLeftSource = control is ListView 
					? control.Parent.PointToScreen(control.Location) 
					: control.PointToScreen(Point.Empty);

				g.CopyFromScreen(upperLeftSource, Point.Empty, control.Bounds.Size);
			}
			return bitmap;
		}
	}
}
