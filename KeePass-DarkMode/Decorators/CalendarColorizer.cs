using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace KeePassDarkMode.Decorators
{
	// The DateTimePicker's month calendar (SysMonthCal32) ignores the theme's
	// colors while visual styles own it. Drop the theme on the calendar window
	// and push our palette through MCM_SETCOLOR - this covers the header bar,
	// day grid, grayed days and the "Today:" footer.
	internal static class CalendarColorizer
	{
		private const int DTM_GETMONTHCAL = 0x1008;
		private const int MCM_SETCOLOR = 0x100A;
		private const int WM_PAINT = 0x000F;

		private const int MCSC_BACKGROUND = 0;
		private const int MCSC_TEXT = 1;
		private const int MCSC_TITLEBK = 2;
		private const int MCSC_TITLETEXT = 3;
		private const int MCSC_MONTHBK = 4;
		private const int MCSC_TRAILINGTEXT = 5;

		[DllImport("user32.dll", EntryPoint = "SendMessageW")]
		private static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
		private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubClassId);

		public static IntPtr GetCalendar(IntPtr pickerHandle)
		{
			return SendMessage(pickerHandle, DTM_GETMONTHCAL, IntPtr.Zero, IntPtr.Zero);
		}

		public static void Colorize(IntPtr calendar, Color back, Color text, Color titleBack)
		{
			if (calendar == IntPtr.Zero)
				return;

			try
			{
				// Colors only take effect on the unthemed control.
				SetWindowTheme(calendar, "", "");

				SetColor(calendar, MCSC_BACKGROUND, back);
				SetColor(calendar, MCSC_TEXT, text);
				SetColor(calendar, MCSC_TITLEBK, titleBack);
				SetColor(calendar, MCSC_TITLETEXT, text);
				SetColor(calendar, MCSC_MONTHBK, back);
				SetColor(calendar, MCSC_TRAILINGTEXT, Blend(text, back));

				SendMessage(calendar, WM_PAINT, IntPtr.Zero, IntPtr.Zero);
			}
			catch (Exception)
			{
				// Calendar is cosmetic - never fail the dialog over it.
			}
		}

		public static void Restore(IntPtr calendar)
		{
			if (calendar == IntPtr.Zero)
				return;

			try
			{
				// Passing null re-enables visual styles; the themed look wins
				// over the stored MCM colors.
				SetWindowTheme(calendar, null, null);
			}
			catch (Exception)
			{
			}
		}

		private static void SetColor(IntPtr calendar, int index, Color color)
		{
			SendMessage(calendar, MCM_SETCOLOR, new IntPtr(index), new IntPtr(ToColorRef(color)));
		}

		// COLORREF is 0x00BBGGRR.
		private static int ToColorRef(Color color)
		{
			return color.B << 16 | color.G << 8 | color.R;
		}

		private static Color Blend(Color a, Color b)
		{
			return Color.FromArgb(
				(a.R + b.R) / 2, (a.G + b.G) / 2, (a.B + b.B) / 2);
		}
	}
}
