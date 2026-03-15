[CmdletBinding()]
param(
    [Parameter()] [string]$SiteName = "FamilyFinances.Web",
    [Parameter()] [string]$FirewallRuleName = "FamilyFinances.Web.LAN.HTTPS",
    [Parameter()] [int]$HttpsPort = 5443,
    [switch]$AsJson
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$warnings = New-Object System.Collections.Generic.List[string]
$httpsBinding = $null
$firewallRule = $null

try {
    Import-Module WebAdministration -ErrorAction Stop
    $httpsBinding = Get-WebBinding -Name $SiteName -Protocol "https" -ErrorAction Stop |
        Where-Object { $_.bindingInformation -like "*:${HttpsPort}:*" } |
        Select-Object -First 1
}
catch {
    $warnings.Add("IIS status unavailable: $($_.Exception.Message)")
}

try {
    Import-Module NetSecurity -ErrorAction Stop
    $firewallRule = Get-NetFirewallRule -DisplayName $FirewallRuleName -ErrorAction SilentlyContinue
}
catch {
    $warnings.Add("Firewall status unavailable: $($_.Exception.Message)")
}

$hostFromBinding = $null
if ($null -ne $httpsBinding) {
    $parts = $httpsBinding.bindingInformation.Split(":")
    if ($parts.Length -ge 3) {
        $hostFromBinding = $parts[2]
    }
}

$thumbprint = $null
$certSubject = $null
if ($null -ne $httpsBinding) {
    try {
        $sslPathWildcard = "IIS:\SslBindings\0.0.0.0!${HttpsPort}*"
        $sslBinding = Get-ChildItem $sslPathWildcard -ErrorAction Stop | Select-Object -First 1
        if ($null -ne $sslBinding) {
            $thumbprint = $sslBinding.Thumbprint
            $cert = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Thumbprint -eq $thumbprint } | Select-Object -First 1
            if ($null -ne $cert) {
                $certSubject = $cert.Subject
            }
        }
    }
    catch {
        $warnings.Add("Certificate binding status unavailable: $($_.Exception.Message)")
    }
}

$enabled = $false
if ($null -ne $httpsBinding -or $null -ne $firewallRule) {
    $enabled = $true
}

$result = [pscustomobject]@{
    Enabled           = $enabled
    HttpsPort         = $HttpsPort
    HostName          = if ([string]::IsNullOrWhiteSpace($hostFromBinding)) { $env:COMPUTERNAME } else { $hostFromBinding }
    CertificateThumb  = $thumbprint
    CertificateSubject = $certSubject
    FirewallRuleName  = $FirewallRuleName
    FirewallEnabled   = ($null -ne $firewallRule)
    AccessLimited     = ($warnings.Count -gt 0)
    Diagnostic        = if ($warnings.Count -gt 0) { $warnings -join " | " } else { $null }
}
if ($AsJson) {
    return $result | ConvertTo-Json -Depth 6
}
return $result
