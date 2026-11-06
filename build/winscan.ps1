Add-Type @"
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
public delegate bool EnumWindowsProc(IntPtr h, IntPtr l);
public class W {
  [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindowsProc cb, IntPtr l);
  [DllImport("user32.dll")] static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
  [DllImport("user32.dll")] static extern int GetWindowThreadProcessId(IntPtr h, out int p);
  public static List<string> Get(int pid) {
    var r = new List<string>();
    EnumWindows((h, l) => {
      int p; GetWindowThreadProcessId(h, out p);
      if (p == pid) { var sb = new StringBuilder(256); GetWindowText(h, sb, 256); r.Add(sb.ToString()); }
      return true;
    }, IntPtr.Zero);
    return r;
  }
}
"@
$proc = Get-Process KeePass -ErrorAction SilentlyContinue
if ($proc) {
  "KeePass PID: " + $proc.Id
  [W]::Get($proc.Id) | ForEach-Object { "WINDOW: [$_]" }
} else {
  "KeePass laeuft nicht"
}
