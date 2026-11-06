Set-Location C:\build
$p = Start-Process "C:\Program Files\KeePass Password Safe 2\KeePass.exe" -ArgumentList '/plgx-info-file','C:\build\KeePassDarkMode.plgx','--debug' -PassThru
Start-Sleep -Seconds 15
if (-not $p.HasExited) { $p.Kill() }
if (Test-Path C:\build\KeePassDarkMode.plgx.txt) {
  "=== INFO FILE ==="
  Get-Content C:\build\KeePassDarkMode.plgx.txt | Select-Object -First 40
} else {
  "info file nicht erzeugt"
}
