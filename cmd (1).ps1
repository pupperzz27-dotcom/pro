# ===== LONEZA — COMBAT CORE =====
# ระบบเดียวกับ CRI_XD 100%
# RootRva=38214736, Off1=72, Off2=200, ScanDelta=1428, Loop=850

$host.UI.RawUI.WindowTitle = "[ LONEZA ] - COMBAT CORE"
$host.UI.RawUI.BackgroundColor = "Black"
$host.UI.RawUI.ForegroundColor = "White"
Clear-Host

# ===== P/Invoke =====
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class Mem {
    [DllImport("kernel32.dll")] public static extern IntPtr OpenProcess(uint a, bool b, int pid);
    [DllImport("kernel32.dll")] public static extern bool ReadProcessMemory(IntPtr h, IntPtr addr, byte[] buf, uint size, out IntPtr read);
    [DllImport("kernel32.dll")] public static extern bool WriteProcessMemory(IntPtr h, IntPtr addr, byte[] buf, uint size, out IntPtr written);
    [DllImport("kernel32.dll")] public static extern bool VirtualProtectEx(IntPtr h, IntPtr addr, UIntPtr size, uint prot, out uint old);
    [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr h);
    [DllImport("user32.dll", CharSet=CharSet.Auto)] public static extern IntPtr FindWindow(string cls, string wnd);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
}
"@ -ErrorAction SilentlyContinue 2>$null

# ===== FUNCTIONS =====

function Write-Banner {
    Clear-Host
    Write-Host ""
    Write-Host "  ██╗      ██████╗ ███╗   ██╗███████╗███████╗ █████╗ " -ForegroundColor Cyan
    Write-Host "  ██║     ██╔═══██╗████╗  ██║██╔════╝╚════██║██╔══██╗" -ForegroundColor Cyan
    Write-Host "  ██║     ██║   ██║██╔██╗ ██║█████╗      ██╔╝███████║" -ForegroundColor Blue
    Write-Host "  ██║     ██║   ██║██║╚██╗██║██╔══╝     ██╔╝ ██╔══██║" -ForegroundColor Blue
    Write-Host "  ███████╗╚██████╔╝██║ ╚████║███████╗   ██║  ██║  ██║" -ForegroundColor DarkBlue
    Write-Host "  ╚══════╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝  ╚═╝  ╚═╝" -ForegroundColor DarkBlue
    Write-Host ""
    Write-Host "              Five-M Critical & Combat Core" -ForegroundColor DarkCyan
    Write-Host ""
}

function Get-FiveMPid {
    # Try grcWindow first
    $hwnd = [Mem]::FindWindow("grcWindow", $null)
    if ($hwnd -ne [IntPtr]::Zero) {
        $pid2 = 0
        [Mem]::GetWindowThreadProcessId($hwnd, [ref]$pid2) | Out-Null
        if ($pid2 -ne 0) { return $pid2 }
    }
    # Fallback process names
    $names = @("FiveM_b3407_GTAProcess","FiveM_b3258_GTAProcess","FiveM_b3095_GTAProcess",
               "FiveM_b2944_GTAProcess","FiveM_b2802_GTAProcess","FiveM_b2699_GTAProcess",
               "FiveM_b2612_GTAProcess","FiveM_b2545_GTAProcess","FiveM_b2372_GTAProcess",
               "FiveM_b2189_GTAProcess","FiveM_b2060_GTAProcess","FiveM_b1604_GTAProcess",
               "FiveM_GTAProcess","GTAProcess","FiveM","GTA5")
    foreach ($n in $names) {
        $p = Get-Process -Name $n -ErrorAction SilentlyContinue
        if ($p) { return [int]$p[0].Id }
    }
    return 0
}

function ReadPtr($hProc, [uint64]$addr) {
    $buf = New-Object byte[] 8
    $read = [IntPtr]::Zero
    if ([Mem]::ReadProcessMemory($hProc, [IntPtr]$addr, $buf, 8, [ref]$read)) {
        return [BitConverter]::ToUInt64($buf, 0)
    }
    return 0
}

function Get-ScanBase($hProc, [uint64]$modBase) {
    # supportedBuilds: RootRva=38214736, Off1=72, Off2=200, ScanDelta=1428
    $rootRva  = 38214736UL
    $off1     = 72UL
    $off2     = 200UL
    $scanDelta = 1428UL

    $p1 = ReadPtr $hProc ($modBase + $rootRva)
    $p2 = ReadPtr $hProc ($p1 + $off1)
    $p3 = ReadPtr $hProc ($p2 + $off2)

    if ($p3 -ne 0 -and $p3 -ne ($p2 + $off2)) {
        return $p3 + $scanDelta
    }
    # fallback
    return $modBase + 38214736UL
}

function MatchesTarget([float]$v) {
    # targetBits — known crit float signatures
    $targets = @(-0.10f, -0.20f, -0.30f, -0.33f, -0.36f, -0.38f, -0.40f)
    foreach ($t in $targets) {
        if ([Math]::Abs($v - $t) -lt 0.001 -or [Math]::Abs($v + $t) -lt 0.001) {
            return $true
        }
    }
    return $false
}

function WriteFloatRwx($hProc, [uint64]$addr, [float]$val) {
    $bytes = [BitConverter]::GetBytes($val)
    $old = 0
    [Mem]::VirtualProtectEx($hProc, [IntPtr]$addr, [UIntPtr]4, 0x40, [ref]$old) | Out-Null
    $written = [IntPtr]::Zero
    $ok = [Mem]::WriteProcessMemory($hProc, [IntPtr]$addr, $bytes, 4, [ref]$written)
    [Mem]::VirtualProtectEx($hProc, [IntPtr]$addr, [UIntPtr]4, $old, [ref]$old) | Out-Null
    return $ok
}

function Apply-Mode([int]$mode) {
    $modeValues = @{1=-0.10f; 2=-0.20f; 3=-0.30f; 4=-0.33f; 5=-0.36f; 6=-0.38f; 7=-0.40f}
    $patchVal = $modeValues[$mode]

    $pid2 = Get-FiveMPid
    if ($pid2 -eq 0) {
        Write-Host "  [!] FiveM not running" -ForegroundColor Red
        return
    }

    $hProc = [Mem]::OpenProcess(0x1F0FFF, $false, $pid2)
    if ($hProc -eq [IntPtr]::Zero) {
        $hProc = [Mem]::OpenProcess(0x438, $false, $pid2)
    }
    if ($hProc -eq [IntPtr]::Zero) {
        Write-Host "  [!] Need Admin Rights" -ForegroundColor Red
        return
    }

    # Get module base
    $proc = Get-Process -Id $pid2 -ErrorAction SilentlyContinue
    $modBase = 0UL
    if ($proc) {
        foreach ($m in $proc.Modules) {
            if ($m.ModuleName -match "GTAProcess|GTA5") {
                $modBase = [uint64]$m.BaseAddress.ToInt64()
                break
            }
        }
        if ($modBase -eq 0) { $modBase = [uint64]$proc.MainModule.BaseAddress.ToInt64() }
    }

    Write-Host "  [+] PID: $pid2 | ModBase: 0x$($modBase.ToString('X'))" -ForegroundColor Green

    $scanBase = Get-ScanBase $hProc $modBase
    Write-Host "  [+] ScanBase: 0x$($scanBase.ToString('X'))" -ForegroundColor DarkCyan
    Write-Host "  [+] Patching Mode $mode ($patchVal)..." -ForegroundColor Cyan

    $count = 0
    $buf = New-Object byte[] 4
    $read = [IntPtr]::Zero

    for ($i = 0; $i -lt 850; $i++) {
        $addr = $scanBase + [uint64]($i * 4)
        if ([Mem]::ReadProcessMemory($hProc, [IntPtr]$addr, $buf, 4, [ref]$read)) {
            $v = [BitConverter]::ToSingle($buf, 0)
            if (MatchesTarget $v) {
                if (WriteFloatRwx $hProc $addr $patchVal) { $count++ }
            }
        }
    }

    [Mem]::CloseHandle($hProc) | Out-Null

    if ($count -gt 0) {
        Write-Host "  [+] Mode $mode LOCKED — $count offset(s) patched" -ForegroundColor Green
    } else {
        Write-Host "  [!] No offsets found — FiveM อาจยังไม่โหลดเต็ม" -ForegroundColor Yellow
    }
}

function Super-Boost {
    Write-Host "  [+] TCP Optimization..." -ForegroundColor Cyan
    reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v NetworkThrottlingIndex /t REG_DWORD /d 4294967295 /f 2>$null | Out-Null
    reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v SystemResponsiveness /t REG_DWORD /d 0 /f 2>$null | Out-Null
    reg add "HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" /v Tcp1323Opts /t REG_DWORD /d 1 /f 2>$null | Out-Null
    reg add "HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" /v TcpTimedWaitDelay /t REG_DWORD /d 30 /f 2>$null | Out-Null
    reg add "HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces" /v TcpAckFrequency /t REG_DWORD /d 1 /f 2>$null | Out-Null
    reg add "HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces" /v TCPNoDelay /t REG_DWORD /d 1 /f 2>$null | Out-Null
    netsh int tcp set global autotuninglevel=normal 2>$null | Out-Null
    netsh int tcp set global rss=enabled 2>$null | Out-Null
    netsh int tcp set global fastopen=enabled 2>$null | Out-Null
    ipconfig /flushdns 2>$null | Out-Null
    Write-Host "  [+] RAM Clean..." -ForegroundColor Cyan
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
    Add-Type @"
using System; using System.Runtime.InteropServices;
public class RamUtil { [DllImport("psapi.dll")] public static extern bool EmptyWorkingSet(IntPtr h); }
"@ -ErrorAction SilentlyContinue 2>$null
    Get-Process | ForEach-Object { try { [RamUtil]::EmptyWorkingSet($_.Handle) | Out-Null } catch {} }
    Write-Host "  [+] Super Boost ACTIVE — TCP clean / RAM freed" -ForegroundColor Green
}

# ===== MAIN LOOP =====
while ($true) {
    Write-Banner

    $pid2 = Get-FiveMPid
    if ($pid2 -ne 0) {
        $pname = (Get-Process -Id $pid2 -ErrorAction SilentlyContinue).Name
        Write-Host "  Game Status : " -NoNewline -ForegroundColor Gray
        Write-Host "Connected • $pname (PID: $pid2)" -ForegroundColor Green
    } else {
        Write-Host "  Game Status : " -NoNewline -ForegroundColor Gray
        Write-Host "FiveM Not Running" -ForegroundColor Yellow
    }

    Write-Host ""
    Write-Host "  ─────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "  [1] Mode 1  •  -0.10f  — ล็อคเบา (Legit)" -ForegroundColor White
    Write-Host "  [2] Mode 2  •  -0.20f  — ล็อคศีรษะนุ่มนวล" -ForegroundColor White
    Write-Host "  [3] Mode 3  •  -0.30f  — คริ 100% [DEFAULT]" -ForegroundColor Cyan
    Write-Host "  ─────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "  [B] Super Boost  — TCP + RAM Clean" -ForegroundColor Magenta
    Write-Host "  [C] Cleaner      — ลบ trace 99%" -ForegroundColor Yellow
    Write-Host "  [0] Exit" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Select: " -NoNewline -ForegroundColor White
    $choice = Read-Host

    switch ($choice.ToUpper()) {
        "1" { Apply-Mode 1; Read-Host "  Enter เพื่อกลับ" }
        "2" { Apply-Mode 2; Read-Host "  Enter เพื่อกลับ" }
        "3" { Apply-Mode 3; Read-Host "  Enter เพื่อกลับ" }
        "B" { Super-Boost; Read-Host "  Enter เพื่อกลับ" }
        "C" {
            iex (iwr -UseBasicParsing 'https://raw.githubusercontent.com/pupperzz27-dotcom/pro/refs/heads/main/cleaner.ps1').Content
            Read-Host "  Enter เพื่อกลับ"
        }
        "0" { exit }
        default { Write-Host "  [!] Invalid" -ForegroundColor Red; Start-Sleep 1 }
    }
}
