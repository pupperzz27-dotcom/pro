# ========== LONEZA CLEANER ==========
# ลบ trace 99% — forensics, history, prefetch, registry, logs
# Run as Administrator
# ====================================

param([switch]$Silent)

function Log($msg) { if (-not $Silent) { Write-Host "[+] $msg" -ForegroundColor Green } }
function Warn($msg) { if (-not $Silent) { Write-Host "[!] $msg" -ForegroundColor Yellow } }

# Admin check
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]"Administrator")) {
    Start-Process powershell "-ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb runas
    exit
}

if (-not $Silent) {
    Clear-Host
    Write-Host "=====================================================================" -ForegroundColor Cyan
    Write-Host "         LONEZA CLEANER" -ForegroundColor Cyan
    Write-Host "         ลบ trace 99% — forensics safe" -ForegroundColor Cyan
    Write-Host "=====================================================================" -ForegroundColor Cyan
    Write-Host ""
}

# ===== 1. TEMP & CACHE =====
Log "Clearing TEMP folders..."
$tempPaths = @(
    $env:TEMP,
    $env:TMP,
    "C:\Windows\Temp",
    "C:\Windows\Prefetch",
    "$env:LOCALAPPDATA\Temp",
    "$env:LOCALAPPDATA\Microsoft\Windows\INetCache",
    "$env:LOCALAPPDATA\Microsoft\Windows\WebCache",
    "$env:APPDATA\Microsoft\Windows\Recent",
    "$env:LOCALAPPDATA\Microsoft\Windows\History"
)
foreach ($p in $tempPaths) {
    if (Test-Path $p) {
        Get-ChildItem $p -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# ===== 2. PREFETCH =====
Log "Clearing Prefetch..."
Get-ChildItem "C:\Windows\Prefetch" -Filter "*.pf" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue

# ===== 3. EVENT LOGS =====
Log "Clearing Event Logs..."
$logs = @("Application","Security","System","Setup","Microsoft-Windows-PowerShell/Operational",
          "Microsoft-Windows-TaskScheduler/Operational","Microsoft-Windows-TerminalServices-LocalSessionManager/Operational",
          "Microsoft-Windows-Windows Defender/Operational","Microsoft-Windows-Bits-Client/Operational")
foreach ($log in $logs) {
    try { wevtutil cl $log 2>$null } catch {}
}
# Clear all logs
wevtutil el 2>$null | ForEach-Object { try { wevtutil cl $_ 2>$null } catch {} }

# ===== 4. RECENT FILES (UserAssist) =====
Log "Clearing Recent & UserAssist..."
# Recent files
$recentPaths = @(
    "$env:APPDATA\Microsoft\Windows\Recent",
    "$env:APPDATA\Microsoft\Windows\Recent\AutomaticDestinations",
    "$env:APPDATA\Microsoft\Windows\Recent\CustomDestinations"
)
foreach ($p in $recentPaths) {
    if (Test-Path $p) {
        Get-ChildItem $p -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# UserAssist (run history)
$uaKeys = @(
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist"
)
foreach ($key in $uaKeys) {
    if (Test-Path $key) {
        Get-ChildItem $key -ErrorAction SilentlyContinue | ForEach-Object {
            Remove-ItemProperty -Path "$($_.PSPath)\Count" -Name * -Force -ErrorAction SilentlyContinue
        }
    }
}

# ===== 5. RUN HISTORY (MRU) =====
Log "Clearing Run/MRU history..."
$mruKeys = @(
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU",
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths",
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSavePidlMRU",
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU",
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRULegacy",
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs",
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit"
)
foreach ($key in $mruKeys) {
    if (Test-Path $key) {
        Remove-ItemProperty -Path $key -Name * -Force -ErrorAction SilentlyContinue
    }
}

# ===== 6. POWERSHELL HISTORY =====
Log "Clearing PowerShell history..."
$psHistory = "$env:APPDATA\Microsoft\Windows\PowerShell\PSReadLine\ConsoleHost_history.txt"
if (Test-Path $psHistory) { Clear-Content $psHistory -Force -ErrorAction SilentlyContinue }
# Also clear current session
try { [Microsoft.PowerShell.PSConsoleReadLine]::ClearHistory() } catch {}

# ===== 7. CMD HISTORY =====
Log "Clearing CMD history..."
# doskey history can't be cleared from script but we can clear registry
Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU" -Name * -Force -ErrorAction SilentlyContinue

# ===== 8. BROWSER DOWNLOAD HISTORY =====
Log "Clearing browser history..."
# Chrome
$chromePaths = @(
    "$env:LOCALAPPDATA\Google\Chrome\User Data\Default\History",
    "$env:LOCALAPPDATA\Google\Chrome\User Data\Default\History-journal",
    "$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Downloads"
)
# Edge
$edgePaths = @(
    "$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\History",
    "$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\History-journal"
)
# Firefox
$ffProfile = "$env:APPDATA\Mozilla\Firefox\Profiles"
$firefoxPaths = @()
if (Test-Path $ffProfile) {
    Get-ChildItem $ffProfile -Directory | ForEach-Object {
        $firefoxPaths += "$($_.FullName)\places.sqlite"
        $firefoxPaths += "$($_.FullName)\downloads.sqlite"
    }
}
foreach ($p in ($chromePaths + $edgePaths + $firefoxPaths)) {
    if (Test-Path $p) { Remove-Item $p -Force -ErrorAction SilentlyContinue }
}

# ===== 9. EXTRACT HISTORY (WinRAR, 7-Zip) =====
Log "Clearing extract history..."
$extractKeys = @(
    "HKCU:\Software\WinRAR\ArcHistory",
    "HKCU:\Software\WinRAR\DialogEditHistory\ExtrPath",
    "HKCU:\Software\WinRAR\DialogEditHistory\ArcName",
    "HKCU:\Software\7-Zip\Compression",
    "HKCU:\Software\7-Zip\Extraction"
)
foreach ($key in $extractKeys) {
    if (Test-Path $key) {
        Remove-ItemProperty -Path $key -Name * -Force -ErrorAction SilentlyContinue
    }
}

# ===== 10. JOURNAL / USN JOURNAL =====
Log "Clearing USN Journal..."
try {
    fsutil usn deletejournal /D C: 2>$null
    fsutil usn createjournal m=0x800000 a=0x100000 C: 2>$null
} catch {}

# ===== 11. DNS CACHE =====
Log "Flushing DNS cache..."
ipconfig /flushdns 2>$null | Out-Null

# ===== 12. THUMBNAIL CACHE =====
Log "Clearing thumbnail cache..."
$thumbPath = "$env:LOCALAPPDATA\Microsoft\Windows\Explorer"
if (Test-Path $thumbPath) {
    Get-ChildItem $thumbPath -Filter "thumbcache_*.db" -Force -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
}

# ===== 13. WINDOWS SEARCH INDEX =====
Log "Clearing Search history..."
Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\WordWheelQuery" -Name * -Force -ErrorAction SilentlyContinue

# ===== 14. SHELLBAGS =====
Log "Clearing ShellBags..."
$shellbagKeys = @(
    "HKCU:\Software\Microsoft\Windows\Shell\BagMRU",
    "HKCU:\Software\Microsoft\Windows\Shell\Bags",
    "HKCU:\Software\Microsoft\Windows\ShellNoRoam\BagMRU",
    "HKCU:\Software\Microsoft\Windows\ShellNoRoam\Bags"
)
foreach ($key in $shellbagKeys) {
    if (Test-Path $key) { Remove-Item $key -Recurse -Force -ErrorAction SilentlyContinue }
}

# ===== 15. PAGEFILE (optional — wipe on shutdown) =====
Log "Setting pagefile wipe on shutdown..."
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v ClearPageFileAtShutdown /t REG_DWORD /d 1 /f 2>$null | Out-Null

if (-not $Silent) {
    Write-Host ""
    Write-Host "[✓] Clean complete — trace ลบแล้ว 99%" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  Press any key to exit..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}
