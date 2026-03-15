[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string]$PhysicalPath,
    [Parameter()] [string]$SiteName = "FamilyFinances.Web",
    [Parameter()] [string]$AppPoolName = "FamilyFinances.Web.AppPool",
    [Parameter()] [int]$LocalPort = 5019
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\constants.ps1")
Assert-Administrator

Import-Module WebAdministration

if (-not (Test-Path $PhysicalPath)) {
    throw "Web physical path does not exist: $PhysicalPath"
}

if (-not (Test-Path ("IIS:\AppPools\$AppPoolName"))) {
    New-WebAppPool -Name $AppPoolName | Out-Null
}

Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name processModel.identityType -Value 4

if (-not (Get-Website -Name $SiteName -ErrorAction SilentlyContinue)) {
    New-Website -Name $SiteName -PhysicalPath $PhysicalPath -Port $LocalPort -IPAddress "*" -HostHeader "localhost" | Out-Null
}
else {
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $PhysicalPath
}

Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName

$bindings = Get-WebBinding -Name $SiteName
foreach ($binding in $bindings) {
    if ($binding.protocol -eq "http" -and $binding.bindingInformation -eq "*:${LocalPort}:localhost") {
        continue
    }
    Remove-WebBinding -Name $SiteName -Protocol $binding.protocol -BindingInformation $binding.bindingInformation
}

if (-not (Get-WebBinding -Name $SiteName -Protocol "http" -Port $LocalPort -HostHeader "localhost")) {
    New-WebBinding -Name $SiteName -Protocol "http" -Port $LocalPort -IPAddress "*" -HostHeader "localhost" | Out-Null
}

Start-WebAppPool -Name $AppPoolName | Out-Null
Start-Website -Name $SiteName | Out-Null

[pscustomobject]@{
    SiteName      = $SiteName
    AppPoolName   = $AppPoolName
    PhysicalPath  = $PhysicalPath
    LocalEndpoint = "http://localhost:$LocalPort"
}
