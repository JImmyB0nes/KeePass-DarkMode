Get-EventLog Application -Newest 10 -ErrorAction SilentlyContinue |
  Where-Object { $_.EntryType -in 'Error','Warning' } |
  Select-Object TimeGenerated, EntryType, Source, @{n='Msg';e={$_.Message.Substring(0,[Math]::Min(500,$_.Message.Length))}} |
  Format-List
