[CmdletBinding()]
param(
    [Parameter()] [int]$ApiPort = 5084,
    [Parameter()] [int]$WebPort = 5019,
    [Parameter()] [int]$MaxRetries = 60
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Invoke-HealthCheck {
    param(
        [Parameter(Mandatory = $true)] [string]$Uri,
        [Parameter(Mandatory = $true)] [int]$MaxRetries,
        [ref]$LastFailure
    )

    for ($attempt = 1; $attempt -le $MaxRetries; $attempt++) {
        try {
            $response = Invoke-WebRequest -Uri $Uri -Method Get -UseBasicParsing -TimeoutSec 3
            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
                return $true
            }
            $LastFailure.Value = "HTTP $($response.StatusCode)"
        }
        catch {
            $statusCode = $null
            if ($null -ne $_.Exception -and $null -ne $_.Exception.Response) {
                try {
                    $statusCode = [int]$_.Exception.Response.StatusCode
                }
                catch {
                    $statusCode = $null
                }
            }

            # Web requests returning 4xx still prove endpoint reachability.
            if ($null -ne $statusCode -and $statusCode -ge 400 -and $statusCode -lt 500) {
                return $true
            }

            $LastFailure.Value = $_.Exception.Message
            Start-Sleep -Seconds 1
        }
    }

    return $false
}

$apiLastFailure = $null
$apiOk = Invoke-HealthCheck -Uri "http://127.0.0.1:$ApiPort/health" -MaxRetries $MaxRetries -LastFailure ([ref]$apiLastFailure)
if (-not $apiOk) {
    throw "Post-install health check failed for API endpoint. Last error: $apiLastFailure"
}

$webLastFailure = $null
$webOk = Invoke-HealthCheck -Uri "http://localhost:$WebPort" -MaxRetries $MaxRetries -LastFailure ([ref]$webLastFailure)
if (-not $webOk) {
    $siteState = "unknown"
    $appPoolState = "unknown"
    $aspNetCoreModuleInstalled = $false

    try {
        Import-Module WebAdministration -ErrorAction Stop | Out-Null
        $site = Get-Website -Name "FamilyFinances.Web" -ErrorAction SilentlyContinue
        if ($null -ne $site) {
            $siteState = $site.State
        }

        $pool = Get-WebAppPoolState -Name "FamilyFinances.Web.AppPool" -ErrorAction SilentlyContinue
        if ($null -ne $pool -and $null -ne $pool.Value) {
            $appPoolState = $pool.Value
        }

        $aspNetCoreModuleInstalled = $null -ne (Get-WebGlobalModule -Name "AspNetCoreModuleV2" -ErrorAction SilentlyContinue)
    }
    catch {
        # Keep diagnostic defaults; primary failure is the endpoint check itself.
    }

    throw ("Post-install health check failed for Web endpoint. " +
           "Last error: $webLastFailure. " +
           "SiteState=$siteState. AppPoolState=$appPoolState. AspNetCoreModuleV2Installed=$aspNetCoreModuleInstalled.")
}

[pscustomobject]@{
    ApiEndpoint = "http://127.0.0.1:$ApiPort/health"
    WebEndpoint = "http://localhost:$WebPort"
    Healthy     = $true
}
