param(
    [Parameter(Mandatory = $false)]
    [string] $PolicyPath = ".codex/opsx-gstack-policy.json"
)

Set-StrictMode -Version Latest

. "$PSScriptRoot/opsx-gstack-policy.ps1"

function Assert-True {
    param(
        [Parameter(Mandatory = $true)]
        [bool] $Condition,

        [Parameter(Mandatory = $true)]
        [string] $Message
    )

    if (-not $Condition) {
        throw "Assertion failed: $Message"
    }
}

function Assert-False {
    param(
        [Parameter(Mandatory = $true)]
        [bool] $Condition,

        [Parameter(Mandatory = $true)]
        [string] $Message
    )

    if ($Condition) {
        throw "Assertion failed: $Message"
    }
}

function Assert-Equal {
    param(
        [Parameter(Mandatory = $true)]
        [object] $Expected,

        [Parameter(Mandatory = $true)]
        [object] $Actual,

        [Parameter(Mandatory = $true)]
        [string] $Message
    )

    if ($Expected -ne $Actual) {
        throw "Assertion failed: $Message (expected '$Expected', got '$Actual')."
    }
}

$policy = Read-JsonFile -Path $PolicyPath
$validation = Test-OpsxGstackPolicyObject -Policy $policy
Assert-True -Condition $validation.IsValid -Message "Default repository policy must validate."

$invalidPolicy = [pscustomobject]@{
    version   = 1
    mode      = "assist"
    allowlist = [pscustomobject]@{
        explore = @("gstack-plan-eng-review")
        apply   = @("gstack-review", "gstack-ship")
        verify  = @("gstack-review")
    }
    blocklist = @(
        "gstack-ship",
        "gstack-land-and-deploy",
        "gstack-setup-deploy"
    )
    strict = [pscustomobject]@{
        requiredVerifyGates = @("gstack-review")
    }
    browse = [pscustomobject]@{
        enabled                 = $false
        allowedInPhases         = @("apply")
        assistOnly              = $true
        requiresExplicitRequest = $true
        advisoryOnly            = $true
    }
    confirmation = [pscustomobject]@{
        enabled = $true
        mode    = "ask-per-invocation"
    }
}

$invalidResult = Test-OpsxGstackPolicyObject -Policy $invalidPolicy
Assert-False -Condition $invalidResult.IsValid -Message "Policy with blocked skill in allowlist must fail validation."

$strictFailVerdict = Get-OpsxGstackVerifyVerdict -Mode "strict" -RequiredVerifyGates @("gstack-review") -GateResults @(
    [pscustomobject]@{
        Skill    = "gstack-review"
        Status   = "fail"
        Severity = "critical"
    }
)

Assert-False -Condition $strictFailVerdict.Ready -Message "Strict mode must block on critical failure for required gate."
Assert-True -Condition ($strictFailVerdict.BlockingReasons.Count -gt 0) -Message "Strict mode failure must provide at least one blocking reason."

$assistFailVerdict = Get-OpsxGstackVerifyVerdict -Mode "assist" -RequiredVerifyGates @("gstack-review") -GateResults @(
    [pscustomobject]@{
        Skill    = "gstack-review"
        Status   = "fail"
        Severity = "critical"
    }
)

Assert-True -Condition $assistFailVerdict.Ready -Message "Assist mode must stay advisory even on critical failures."
Assert-Equal -Expected 0 -Actual $assistFailVerdict.BlockingReasons.Count -Message "Assist mode must not produce blocking reasons."

$strictPassVerdict = Get-OpsxGstackVerifyVerdict -Mode "strict" -RequiredVerifyGates @("gstack-review", "gstack-cso") -GateResults @(
    [pscustomobject]@{
        Skill    = "gstack-review"
        Status   = "pass"
        Severity = "critical"
    },
    [pscustomobject]@{
        Skill    = "gstack-cso"
        Status   = "pass"
        Severity = "critical"
    }
)

Assert-True -Condition $strictPassVerdict.Ready -Message "Strict mode must pass when all required gates pass."
Assert-Equal -Expected 0 -Actual $strictPassVerdict.BlockingReasons.Count -Message "Strict pass path must have zero blocking reasons."

$missingConfirmationPolicy = [pscustomobject]@{
    version   = 1
    mode      = "assist"
    allowlist = [pscustomobject]@{
        explore = @("gstack-plan-eng-review")
        apply   = @("gstack-review")
        verify  = @("gstack-review")
    }
    blocklist = @(
        "gstack-ship",
        "gstack-land-and-deploy",
        "gstack-setup-deploy"
    )
}

$missingConfirmationResult = Test-OpsxGstackPolicyObject -Policy $missingConfirmationPolicy
Assert-False -Condition $missingConfirmationResult.IsValid -Message "Policy missing confirmation object must fail validation."

Write-Host "OpenSpec gstack policy tests passed." -ForegroundColor Green
