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
