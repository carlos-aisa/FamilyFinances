[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [object]$Enabled,
    [Parameter()] [string]$SiteName = "FamilyFinances.Web",
    [Parameter()] [string]$FirewallRuleName = "FamilyFinances.Web.LAN.HTTPS",
    [Parameter()] [int]$HttpsPort = 5443,
    [Parameter()] [string]$HostName = "",
    [Parameter()] [object]$RegenerateCertificate = $false,
    [switch]$AsJson
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\constants.ps1")
Assert-Administrator
Import-Module WebAdministration
Import-Module NetSecurity

function Convert-ToBoolean {
    param(
        [Parameter(Mandatory = $true)] [object]$Value,
        [Parameter()] [string]$ParameterName = "value"
    )

    if ($Value -is [bool]) {
        return [bool]$Value
    }

    if ($Value -is [System.Management.Automation.SwitchParameter]) {
        return [bool]$Value.IsPresent
    }

    if ($Value -is [int]) {
        if ($Value -eq 1) { return $true }
        if ($Value -eq 0) { return $false }
    }

    $text = [string]$Value
    if ([string]::IsNullOrWhiteSpace($text)) {
        throw "Parameter '$ParameterName' cannot be empty."
    }

    switch ($text.Trim().ToLowerInvariant()) {
        "true"  { return $true }
        "false" { return $false }
        "1"     { return $true }
        "0"     { return $false }
        "yes"   { return $true }
        "no"    { return $false }
        default { throw "Parameter '$ParameterName' value '$text' is not a valid boolean." }
    }
}

function Ensure-RootCertificate {
    param([string]$RootSubject)

    # Keep signer cert in LocalMachine\My (with private key) and trust the same cert in Root.
    $rootSigner = Get-ChildItem Cert:\LocalMachine\My |
        Where-Object { $_.Subject -eq $RootSubject -and $_.HasPrivateKey } |
        Sort-Object NotAfter -Descending |
        Select-Object -First 1

    if ($null -eq $rootSigner) {
        $rootSigner = New-SelfSignedCertificate `
            -Type Custom `
            -Subject $RootSubject `
            -KeyAlgorithm RSA `
            -KeyLength 4096 `
            -HashAlgorithm SHA256 `
            -KeyExportPolicy Exportable `
            -CertStoreLocation "Cert:\LocalMachine\My" `
            -NotAfter (Get-Date).AddYears(10) `
            -TextExtension @("2.5.29.19={critical}{text}ca=TRUE")
    }

    $trustedRoot = Get-ChildItem Cert:\LocalMachine\Root |
        Where-Object { $_.Thumbprint -eq $rootSigner.Thumbprint } |
        Select-Object -First 1

    if ($null -eq $trustedRoot) {
        $rootStore = [System.Security.Cryptography.X509Certificates.X509Store]::new(
            "Root",
            [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine)

        try {
            $rootStore.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
            $rootStore.Add($rootSigner)
        }
        finally {
            $rootStore.Close()
        }
    }

    return $rootSigner
}

function Ensure-ServerCertificate {
    param(
        [Parameter(Mandatory = $true)] [System.Security.Cryptography.X509Certificates.X509Certificate2]$RootCert,
        [Parameter(Mandatory = $true)] [string]$ServerHost,
        [switch]$ForceRotate
    )

    $existing = Get-ChildItem Cert:\LocalMachine\My |
        Where-Object { $_.Subject -eq "CN=$ServerHost" } |
        Sort-Object NotAfter -Descending |
        Select-Object -First 1

    if ($null -ne $existing -and -not $ForceRotate) {
        return $existing
    }

    return New-SelfSignedCertificate `
        -DnsName @($ServerHost, "localhost") `
        -Signer $RootCert `
        -CertStoreLocation "Cert:\LocalMachine\My" `
        -NotAfter (Get-Date).AddYears(3) `
        -KeyExportPolicy Exportable `
        -FriendlyName "FamilyFinances LAN Certificate"
}

function Set-IisHttpsBinding {
    param(
        [Parameter(Mandatory = $true)] [string]$SiteName,
        [Parameter(Mandatory = $true)] [int]$Port,
        [Parameter(Mandatory = $true)] [string]$HostHeader,
        [Parameter(Mandatory = $true)] [string]$Thumbprint
    )

    $existing = Get-WebBinding -Name $SiteName -Protocol "https" -ErrorAction SilentlyContinue |
        Where-Object { $_.bindingInformation -eq "*:${Port}:${HostHeader}" }
    if ($null -eq $existing) {
        New-WebBinding -Name $SiteName -Protocol "https" -Port $Port -IPAddress "*" -HostHeader $HostHeader | Out-Null
    }

    $sslPath = if ([string]::IsNullOrWhiteSpace($HostHeader)) {
        "IIS:\SslBindings\0.0.0.0!$Port"
    }
    else {
        "IIS:\SslBindings\0.0.0.0!$Port!$HostHeader"
    }

    # Idempotent replace: remove first (if exists) and retry on IIS provider race/visibility quirks.
    Remove-Item $sslPath -Force -ErrorAction SilentlyContinue

    try {
        New-Item $sslPath -Thumbprint $Thumbprint -SSLFlags 0 -ErrorAction Stop | Out-Null
    }
    catch {
        # Some IIS setups keep stale entries not visible via Test-Path.
        $sslCandidates = @(
            ("IIS:\SslBindings\0.0.0.0!{0}!{1}" -f $Port, $HostHeader),
            ("IIS:\SslBindings\0.0.0.0!{0}" -f $Port)
        )

        foreach ($candidate in $sslCandidates) {
            Remove-Item $candidate -Force -ErrorAction SilentlyContinue
        }

        New-Item $sslPath -Thumbprint $Thumbprint -SSLFlags 0 -Force | Out-Null
    }
}

if ($HttpsPort -eq 5084) {
    throw "HTTPS LAN port cannot match API loopback port (5084)."
}

$effectiveHost = if ([string]::IsNullOrWhiteSpace($HostName)) { $env:COMPUTERNAME } else { $HostName }
$defaults = Get-InstallerDefaults
$enabledValue = Convert-ToBoolean -Value $Enabled -ParameterName "Enabled"
$regenerateValue = Convert-ToBoolean -Value $RegenerateCertificate -ParameterName "RegenerateCertificate"

if ($enabledValue) {
    $root = Ensure-RootCertificate -RootSubject $defaults.LocalRootCaSubject
    $serverCert = Ensure-ServerCertificate -RootCert $root -ServerHost $effectiveHost -ForceRotate:$regenerateValue
    Set-IisHttpsBinding -SiteName $SiteName -Port $HttpsPort -HostHeader $effectiveHost -Thumbprint $serverCert.Thumbprint

    if (-not (Get-NetFirewallRule -DisplayName $FirewallRuleName -ErrorAction SilentlyContinue)) {
        New-NetFirewallRule -DisplayName $FirewallRuleName -Direction Inbound -Action Allow -Protocol TCP -LocalPort $HttpsPort -Profile Private | Out-Null
    }

    $result = [pscustomobject]@{
        Enabled          = $true
        HttpsPort        = $HttpsPort
        HostName         = $effectiveHost
        CertificateThumb = $serverCert.Thumbprint
        FirewallRule     = $FirewallRuleName
    }
    if ($AsJson) {
        return $result | ConvertTo-Json -Depth 6
    }
    return $result
}

$bindings = Get-WebBinding -Name $SiteName -Protocol "https" -ErrorAction SilentlyContinue
foreach ($binding in $bindings) {
    if ($binding.bindingInformation -like "*:${HttpsPort}:*") {
        Remove-WebBinding -Name $SiteName -Protocol "https" -BindingInformation $binding.bindingInformation
    }
}

$rule = Get-NetFirewallRule -DisplayName $FirewallRuleName -ErrorAction SilentlyContinue
if ($null -ne $rule) {
    Remove-NetFirewallRule -DisplayName $FirewallRuleName
}

$result = [pscustomobject]@{
    Enabled      = $false
    HttpsPort    = $HttpsPort
    HostName     = $effectiveHost
    FirewallRule = $FirewallRuleName
}
if ($AsJson) {
    return $result | ConvertTo-Json -Depth 6
}
return $result
