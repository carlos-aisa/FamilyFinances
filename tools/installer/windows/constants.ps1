Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:FamilyFinancesInstallerDefaults = @{
    InstallRoot           = Join-Path $env:ProgramFiles "FamilyFinances"
    RuntimeRoot           = Join-Path $env:ProgramData "FamilyFinances"
    SiteName              = "FamilyFinances.Web"
    AppPoolName           = "FamilyFinances.Web.AppPool"
    ApiServiceName        = "FamilyFinances.Api"
    ApiExeName            = "FamilyFinances.Api.exe"
    WebPortLocal          = 5019
    ApiPortLocal          = 5084
    LanHttpsPort          = 5443
    LanFirewallRuleName   = "FamilyFinances.Web.LAN.HTTPS"
    LocalRootCaSubject    = "CN=FamilyFinances Local Root CA"
    LocalServerSubject    = "CN=familyfinances.local"
    DefaultJwtKey         = "PRODUCTION_SECRET_KEY_CHANGE_THIS_IN_REAL_DEPLOYMENT_MIN_64_CHARS_0123456789ABCDEF"
    HostingBundleFileName = "dotnet-hosting-9.0-win.exe"
    HostingBundleDownloadUrl = "https://aka.ms/dotnet/9.0/dotnet-hosting-win.exe"
    HostingBundleRelativePath = "installer-prereqs\dotnet-hosting-9.0-win.exe"
}

function Get-InstallerDefaults {
    [CmdletBinding()]
    param()

    return $script:FamilyFinancesInstallerDefaults.Clone()
}

function Assert-Administrator {
    [CmdletBinding()]
    param()

    $current = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($current)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Administrator privileges are required."
    }
}

function Test-InstallerRebootPending {
    [CmdletBinding()]
    param()

    $sources = New-Object System.Collections.Generic.List[string]

    if (Test-Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending") {
        $sources.Add("ComponentBasedServicing")
    }

    if (Test-Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired") {
        $sources.Add("WindowsUpdate")
    }

    $sessionManagerPath = "HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager"
    try {
        $pendingRename = Get-ItemPropertyValue -Path $sessionManagerPath -Name "PendingFileRenameOperations" -ErrorAction SilentlyContinue
        if ($null -ne $pendingRename -and @($pendingRename).Count -gt 0) {
            $sources.Add("PendingFileRenameOperations")
        }
    }
    catch {
        # Best effort only. Some hosts may restrict or omit this value.
    }

    [pscustomobject]@{
        Pending = $sources.Count -gt 0
        Sources = @($sources)
    }
}

function Test-InstallerRestartNeededValue {
    [CmdletBinding()]
    param(
        [Parameter()] [AllowNull()] [object]$Value
    )

    if ($null -eq $Value) {
        return $false
    }

    $normalized = $Value.ToString().Trim()
    if ([string]::IsNullOrWhiteSpace($normalized)) {
        return $false
    }

    return -not (@("No", "False", "0") -contains $normalized)
}

function Get-InstallerRebootMessage {
    [CmdletBinding()]
    param(
        [Parameter()] [string[]]$Sources = @()
    )

    $sourceSuffix = if ($Sources.Count -gt 0) {
        " Detected state: " + ($Sources -join ", ") + "."
    }
    else {
        ""
    }

    return "Precheck failed. Windows restart is required before FamilyFinances can finish enabling IIS prerequisites. Reboot the machine and rerun setup.$sourceSuffix"
}
