# ===== LONEZA — COMBAT CORE =====
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName Microsoft.VisualBasic

$host.UI.RawUI.WindowTitle = "[ LONEZA ] - COMBAT CORE"
$host.UI.RawUI.BackgroundColor = "Black"
$host.UI.RawUI.ForegroundColor = "White"
Clear-Host

# ===== FUNCTIONS =====

function Write-Banner {
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

function Get-FiveMProcess {
    $names = @("FiveM_b3407_GTAProcess","FiveM_b3258_GTAProcess","FiveM_b3095_GTAProcess",
               "FiveM_b2944_GTAProcess","FiveM_b2802_GTAProcess","FiveM_b2699_GTAProcess",
               "FiveM_b2612_GTAProcess","FiveM_b2545_GTAProcess","FiveM_b2372_GTAProcess",
               "FiveM_b2189_GTAProcess","FiveM_b2060_GTAProcess","FiveM_b1604_GTAProcess",
               "FiveM_GTAProcess","GTAProcess","GTA5","FiveM")
    foreach ($n in $names) {
        $p = Get-Process -Name $n -ErrorAction SilentlyContinue
        if ($p) { return $p }
    }
    return $null
}

function Super-Boost {
    Write-Host "[+] Super Boost — TCP + RAM..." -ForegroundColor Cyan

    # TCP Optimization
    reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v NetworkThrottlingIndex /t REG_DWORD /d 4294967295 /f 2>$null | Out-Null
    reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v SystemResponsiveness /t REG_DWORD /d 0 /f 2>$null | Out-Null
    reg add "HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" /v Tcp1323Opts /t REG_DWORD /d 1 /f 2>$null | Out-Null
    reg add "HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" /v TcpTimedWaitDelay /t REG_DWORD /d 30 /f 2>$null | Out-Null
    netsh int tcp set global autotuninglevel=normal 2>$null | Out-Null
    netsh int tcp set global rss=enabled 2>$null | Out-Null
    netsh int tcp set global fastopen=enabled 2>$null | Out-Null
    ipconfig /flushdns 2>$null | Out-Null

    # RAM Clean
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
    try {
        $mem = [System.Runtime.InteropServices.Marshal]
        Add-Type @"
using System;
using System.Runtime.InteropServices;
public class RAM {
    [DllImport("psapi.dll")] public static extern bool EmptyWorkingSet(IntPtr h);
    [DllImport("kernel32.dll")] public static extern IntPtr GetCurrentProcess();
}
"@ -ErrorAction SilentlyContinue
        [RAM]::EmptyWorkingSet([RAM]::GetCurrentProcess()) | Out-Null
        Get-Process | Where-Object { $_.WorkingSet64 -gt 10MB } | ForEach-Object {
            try { [RAM]::EmptyWorkingSet($_.Handle) | Out-Null } catch {}
        }
    } catch {}

    Write-Host "[+] Super Boost ACTIVE — TCP clean / RAM freed" -ForegroundColor Green
}

function Apply-CritMode($mode, $value) {
    Write-Host "[+] Applying Mode $mode ($value)..." -ForegroundColor Cyan

    $proc = Get-FiveMProcess
    if (-not $proc) {
        Write-Host "[!] FiveM not running — open FiveM first" -ForegroundColor Red
        return
    }

    Write-Host "[+] Found: $($proc.Name) (PID: $($proc.Id))" -ForegroundColor Green

    # Memory write via C# P/Invoke
    Add-Type @"
using System;
using System.Runtime.InteropServices;
public class MemWrite {
    [DllImport("kernel32.dll")] public static extern IntPtr OpenProcess(uint a, bool b, int pid);
    [DllImport("kernel32.dll")] public static extern bool WriteProcessMemory(IntPtr h, IntPtr addr, byte[] buf, uint size, out uint written);
    [DllImport("kernel32.dll")] public static extern bool ReadProcessMemory(IntPtr h, IntPtr addr, byte[] buf, uint size, out uint read);
    [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr h);
    [DllImport("kernel32.dll")] public static extern IntPtr VirtualQueryEx(IntPtr h, IntPtr addr, out MEMORY_BASIC_INFORMATION mbi, uint size);
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION {
        public IntPtr BaseAddress, AllocationBase;
        public uint AllocationProtect, State, Protect, Type;
        public IntPtr RegionSize;
    }
}
"@ -ErrorAction SilentlyContinue 2>$null

    try {
        $hProc = [MemWrite]::OpenProcess(0x1F0FFF, $false, $proc.Id)
        if ($hProc -eq [IntPtr]::Zero) {
            Write-Host "[!] Need Admin — run as Administrator" -ForegroundColor Red
            return
        }

        # Convert float value to bytes
        $floatVal = [float]$value
        $bytes = [BitConverter]::GetBytes($floatVal)

        # AOB scan for critical value pattern in FiveM memory
        # Pattern: scan writable memory regions for the float signature
        $baseAddr = $proc.MainModule.BaseAddress
        $moduleSize = $proc.MainModule.ModuleMemorySize
        $found = 0

        Write-Host "[+] Scanning memory for critical offset..." -ForegroundColor DarkCyan

        $addr = $baseAddr
        $endAddr = [IntPtr]($baseAddr.ToInt64() + $moduleSize)

        while ($addr.ToInt64() -lt $endAddr.ToInt64()) {
            $mbi = New-Object MemWrite+MEMORY_BASIC_INFORMATION
            $ret = [MemWrite]::VirtualQueryEx($hProc, $addr, [ref]$mbi, [System.Runtime.InteropServices.Marshal]::SizeOf($mbi))
            if ($ret -eq [IntPtr]::Zero) { break }

            if ($mbi.State -eq 0x1000 -and ($mbi.Protect -band 0x04 -or $mbi.Protect -band 0x40)) {
                $regionSize = $mbi.RegionSize.ToInt64()
                if ($regionSize -gt 0 -and $regionSize -lt 50MB) {
                    $buf = New-Object byte[] $regionSize
                    $read = 0
                    if ([MemWrite]::ReadProcessMemory($hProc, $addr, $buf, $regionSize, [ref]$read)) {
                        for ($i = 0; $i -lt $read - 4; $i++) {
                            # Look for -0.30f pattern (default crit value)
                            $existing = [BitConverter]::ToSingle($buf, $i)
                            if ([Math]::Abs($existing - (-0.30)) -lt 0.001 -or
                                [Math]::Abs($existing - (-0.10)) -lt 0.001 -or
                                [Math]::Abs($existing - (-0.20)) -lt 0.001 -or
                                [Math]::Abs($existing - (-0.33)) -lt 0.001 -or
                                [Math]::Abs($existing - (-0.36)) -lt 0.001 -or
                                [Math]::Abs($existing - (-0.38)) -lt 0.001 -or
                                [Math]::Abs($existing - (-0.40)) -lt 0.001) {
                                $writeAddr = [IntPtr]($addr.ToInt64() + $i)
                                $written = 0
                                [MemWrite]::WriteProcessMemory($hProc, $writeAddr, $bytes, 4, [ref]$written) | Out-Null
                                $found++
                            }
                        }
                    }
                }
            }
            $addr = [IntPtr]($addr.ToInt64() + [Math]::Max($mbi.RegionSize.ToInt64(), 1))
        }

        [MemWrite]::CloseHandle($hProc) | Out-Null

        if ($found -gt 0) {
            Write-Host "[+] Mode $mode LOCKED — $found offset(s) patched ($value)" -ForegroundColor Green
        } else {
            Write-Host "[!] Offset not found — FiveM build may differ or value already set" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "[!] Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# ===== MAIN MENU =====
while ($true) {
    Clear-Host
    Write-Banner

    # Check FiveM status
    $proc = Get-FiveMProcess
    if ($proc) {
        Write-Host "  Game Status : " -NoNewline -ForegroundColor Gray
        Write-Host "Connected • $($proc.Name) (PID: $($proc.Id))" -ForegroundColor Green
    } else {
        Write-Host "  Game Status : " -NoNewline -ForegroundColor Gray
        Write-Host "FiveM Not Running" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "  ─────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "  [1] Mode 1  •  -0.10f  — ล็อคเบา (Legit)" -ForegroundColor White
    Write-Host "  [2] Mode 2  •  -0.20f  — ล็อคศีรษะนุ่มนวล" -ForegroundColor White
    Write-Host "  [3] Mode 3  •  -0.30f  — คริ 100% [DEFAULT]" -ForegroundColor Cyan
    Write-Host "  ─────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "  [B] Super Boost  — TCP + RAM Clean" -ForegroundColor Magenta
    Write-Host "  [C] Cleaner      — ลบ trace 99%" -ForegroundColor DarkYellow
    Write-Host "  [R] Restore      — คืนค่าเดิม" -ForegroundColor DarkCyan
    Write-Host "  [0] Exit" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Select: " -NoNewline -ForegroundColor White
    $choice = Read-Host

    switch ($choice.ToUpper()) {
        "1" { Apply-CritMode 1 -0.10; Write-Host ""; Write-Host "  Press Enter..." -ForegroundColor Gray; Read-Host }
        "2" { Apply-CritMode 2 -0.20; Write-Host ""; Write-Host "  Press Enter..." -ForegroundColor Gray; Read-Host }
        "3" { Apply-CritMode 3 -0.30; Write-Host ""; Write-Host "  Press Enter..." -ForegroundColor Gray; Read-Host }
        "B" { Super-Boost; Write-Host ""; Write-Host "  Press Enter..." -ForegroundColor Gray; Read-Host }
        "C" {
            Write-Host "[+] Running Cleaner..." -ForegroundColor Cyan
            iex (iwr -UseBasicParsing 'https://raw.githubusercontent.com/pupperzz27-dotcom/pro/refs/heads/main/cleaner.ps1').Content
            Write-Host "  Press Enter..." -ForegroundColor Gray; Read-Host
        }
        "R" {
            $proc = Get-FiveMProcess
            if ($proc) {
                Write-Host "[+] Restoring default values..." -ForegroundColor Yellow
                Apply-CritMode "Restore" -0.30
            } else {
                Write-Host "[!] FiveM not running" -ForegroundColor Red
            }
            Write-Host "  Press Enter..." -ForegroundColor Gray; Read-Host
        }
        "0" { exit }
        default { Write-Host "[!] Invalid" -ForegroundColor Red; Start-Sleep 1 }
    }
}
