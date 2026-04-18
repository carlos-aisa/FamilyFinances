param(
    [Parameter(Mandatory = $false)]
    [string] $PolicyPath = ".codex/opsx-gstack-policy.json",

    [Parameter(Mandatory = $false)]
    [ValidateSet("off", "assist", "strict")]
    [string] $Mode = "off"
)

Set-StrictMode -Version Latest

. "$PSScriptRoot/opsx-gstack-policy.ps1"

$policy = Read-JsonFile -Path $PolicyPath
$validation = Test-OpsxGstackPolicyObject -Policy $policy

if (-not $validation.IsValid) {
    Write-Host "Policy is invalid; dry-run cannot continue." -ForegroundColor Red
    foreach ($error in $validation.Errors) {
        Write-Host " - $error" -ForegroundColor Red
    }
    exit 1
}

$effectiveMode = $Mode
$blocked = @($policy.blocklist)
$confirmationEnabled = ($null -ne $policy.confirmation -and $policy.confirmation.enabled -eq $true)
$confirmationMode = if ($null -ne $policy.confirmation) { [string]$policy.confirmation.mode } else { "unknown" }

Write-Host "Dry run mode: $effectiveMode" -ForegroundColor Cyan
Write-Host "Blocklist: $([string]::Join(', ', $blocked))" -ForegroundColor Cyan
Write-Host "Confirmation: enabled=$confirmationEnabled, mode=$confirmationMode" -ForegroundColor Cyan
Write-Host ""

foreach ($phase in @("explore", "apply", "verify")) {
    $skills = @($policy.allowlist.$phase)
    $filtered = @($skills | Where-Object { $_ -notin $blocked })
    Write-Host "Phase '$phase' candidate skills: $([string]::Join(', ', $skills))"
    Write-Host "Phase '$phase' executable skills: $([string]::Join(', ', $filtered))"

    if ($phase -eq "verify") {
        $gateResults = @(
            [pscustomobject]@{
                Skill    = "gstack-review"
                Status   = "pass"
                Severity = "critical"
            },
            [pscustomobject]@{
                Skill    = "gstack-qa-only"
                Status   = "pass"
                Severity = "critical"
            },
            [pscustomobject]@{
                Skill    = "gstack-cso"
                Status   = "pass"
                Severity = "critical"
            }
        )

        $required = @()
        if ($null -ne $policy.strict -and $null -ne $policy.strict.requiredVerifyGates) {
            $required = @($policy.strict.requiredVerifyGates)
        }

        $verdict = Get-OpsxGstackVerifyVerdict -Mode $effectiveMode -GateResults $gateResults -RequiredVerifyGates $required
        Write-Host "Verify readiness in mode '$effectiveMode': $($verdict.Ready)"
        if ($verdict.BlockingReasons.Count -gt 0) {
            Write-Host "Blocking reasons:" -ForegroundColor Yellow
            foreach ($reason in $verdict.BlockingReasons) {
                Write-Host " - $reason" -ForegroundColor Yellow
            }
        }
    }

    if ($phase -eq "apply" -and ($skills -contains "gstack-browse")) {
        $browseAllowed = $policy.browse.enabled -eq $true -and (($policy.browse.assistOnly -eq $false) -or ($effectiveMode -eq "assist"))
        Write-Host "Browse rule: allowed for apply diagnostics in assist semantics = $browseAllowed"
    }

    Write-Host ""
}

Write-Host "Dry run completed." -ForegroundColor Green
