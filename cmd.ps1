# ===== LONEZA UI =====
$host.UI.RawUI.WindowTitle = "[ LONEZA ] - SHOP"
$host.UI.RawUI.BackgroundColor = "Black"
$host.UI.RawUI.ForegroundColor = "White"
Clear-Host

Write-Host ""
Write-Host "  ██╗      ██████╗ ███╗   ██╗███████╗███████╗ █████╗ " -ForegroundColor Cyan
Write-Host "  ██║     ██╔═══██╗████╗  ██║██╔════╝╚════██║██╔══██╗" -ForegroundColor Cyan
Write-Host "  ██║     ██║   ██║██╔██╗ ██║█████╗      ██╔╝███████║" -ForegroundColor Blue
Write-Host "  ██║     ██║   ██║██║╚██╗██║██╔══╝     ██╔╝ ██╔══██║" -ForegroundColor Blue
Write-Host "  ███████╗╚██████╔╝██║ ╚████║███████╗   ██║  ██║  ██║" -ForegroundColor DarkBlue
Write-Host "  ╚══════╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝  ╚═╝  ╚═╝" -ForegroundColor DarkBlue
Write-Host ""
Write-Host "  [1] Loneza -> Low" -ForegroundColor White
Write-Host "  [2] Loneza -> Medium" -ForegroundColor White
Write-Host "  [3] Loneza -> High" -ForegroundColor White
Write-Host "  [4] Loneza -> Full system" -ForegroundColor White
Write-Host ""
Write-Host "  [0] Exit" -ForegroundColor Gray
Write-Host ""
Write-Host "  Select mode: " -ForegroundColor White -NoNewline

$choice = Read-Host

switch ($choice) {
    "1" { Write-Host "[+] Loading Low mode..." -ForegroundColor Green; Start-Sleep 2 }
    "2" { Write-Host "[+] Loading Medium mode..." -ForegroundColor Green; Start-Sleep 2 }
    "3" { Write-Host "[+] Loading High mode..." -ForegroundColor Green; Start-Sleep 2 }
    "4" { Write-Host "[+] Loading Full system..." -ForegroundColor Green; Start-Sleep 2 }
    "0" { exit }
    default { Write-Host "[!] Invalid selection" -ForegroundColor Red; Start-Sleep 2 }
}
