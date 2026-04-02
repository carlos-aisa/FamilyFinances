Set-StrictMode -Version Latest

$script:OpsxModes = @("off", "assist", "strict")
$script:OpsxPhases = @("explore", "apply", "verify")
$script:RequiredBlockedSkills = @(
    "gstack-ship",
    "gstack-land-and-deploy",
    "gstack-setup-deploy"
)
$script:SupportedConfirmationModes = @(
    "ask-per-invocation",
    "notify-only"
)
$script:KnownSafeSkills = @(
    "gstack-office-hours",
    "gstack-plan-ceo-review",
    "gstack-plan-eng-review",
    "gstack-plan-design-review",
    "gstack-review",
    "gstack-qa",
    "gstack-qa-only",
    "gstack-design-review",
    "gstack-cso",
    "gstack-investigate",
    "gstack-browse",
    "gstack-benchmark"
)

function Read-JsonFile {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "File not found: $Path"
    }

    $raw = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    if ([string]::IsNullOrWhiteSpace($raw)) {
        throw "File is empty: $Path"
    }

    return $raw | ConvertFrom-Json
}

function Get-OpsxGstackDefaultPolicyPath {
    param(
        [Parameter(Mandatory = $false)]
        [string] $RepositoryRoot = (Get-Location).Path
    )

    return Join-Path $RepositoryRoot ".codex/opsx-gstack-policy.json"
}

function Get-OpsxGstackDefaultPresetsPath {
    param(
        [Parameter(Mandatory = $false)]
        [string] $RepositoryRoot = (Get-Location).Path
    )

    return Join-Path $RepositoryRoot ".codex/opsx-gstack-policy.presets.json"
}

function Test-OpsxGstackPolicyObject {
    param(
        [Parameter(Mandatory = $true)]
        [pscustomobject] $Policy
    )

    $errors = New-Object System.Collections.Generic.List[string]
    $warnings = New-Object System.Collections.Generic.List[string]

    if ($null -eq $Policy.version) {
        $errors.Add("Missing 'version' field.")
    }

    if ($null -eq $Policy.mode) {
        $errors.Add("Missing 'mode' field.")
    }
    elseif ($Policy.mode -notin $script:OpsxModes) {
        $errors.Add("Unsupported mode '$($Policy.mode)'. Allowed: off, assist, strict.")
    }

    if ($null -eq $Policy.allowlist) {
        $errors.Add("Missing 'allowlist' object.")
    }
    else {
        foreach ($phase in $script:OpsxPhases) {
            if ($null -eq $Policy.allowlist.$phase) {
                $errors.Add("Missing allowlist phase '$phase'.")
                continue
            }

            $skills = @($Policy.allowlist.$phase)
            if ($skills.Count -eq 0) {
                $warnings.Add("Allowlist phase '$phase' is empty.")
            }

            foreach ($skill in $skills) {
                if ([string]::IsNullOrWhiteSpace([string]$skill)) {
                    $errors.Add("Allowlist phase '$phase' contains an empty skill.")
                    continue
                }
                if ($skill -notin $script:KnownSafeSkills) {
                    $errors.Add("Allowlist phase '$phase' contains unknown/non-safe skill '$skill'.")
                }
            }
        }
    }

    if ($null -eq $Policy.blocklist) {
        $errors.Add("Missing 'blocklist' array.")
    }
    else {
        $blocked = @($Policy.blocklist)
        foreach ($requiredSkill in $script:RequiredBlockedSkills) {
            if ($requiredSkill -notin $blocked) {
                $errors.Add("Required blocked skill '$requiredSkill' is missing from blocklist.")
            }
        }
    }

    if ($null -ne $Policy.allowlist -and $null -ne $Policy.blocklist) {
        $blocked = @($Policy.blocklist)
        foreach ($phase in $script:OpsxPhases) {
            $skills = @($Policy.allowlist.$phase)
            foreach ($skill in $skills) {
                if ($skill -in $blocked) {
                    $errors.Add("Skill '$skill' is both allowlisted and blocklisted (phase '$phase').")
                }
            }
        }
    }

    $hasBrowse = $Policy.PSObject.Properties.Name -contains "browse"
    $hasStrict = $Policy.PSObject.Properties.Name -contains "strict"
    $hasConfirmation = $Policy.PSObject.Properties.Name -contains "confirmation"

    if ($hasBrowse -and $null -ne $Policy.browse) {
        if ($Policy.browse.enabled -eq $true) {
            $applySkills = @($Policy.allowlist.apply)
            if ("gstack-browse" -notin $applySkills) {
                $errors.Add("Browse is enabled but 'gstack-browse' is missing from apply allowlist.")
            }

            $verifySkills = @($Policy.allowlist.verify)
            if ("gstack-browse" -in $verifySkills) {
                $errors.Add("gstack-browse must not appear in verify allowlist.")
            }

            if ($Policy.browse.assistOnly -eq $true -and $Policy.mode -eq "strict") {
                $warnings.Add("Mode is strict while browse is assist-only. Browse invocations must stay disabled in strict flow.")
            }
        }
    }

    if ($hasStrict -and $null -ne $Policy.strict) {
        $requiredGates = @($Policy.strict.requiredVerifyGates)
        $verifySkills = @($Policy.allowlist.verify)
        foreach ($gate in $requiredGates) {
            if ($gate -notin $verifySkills) {
                $errors.Add("Strict required gate '$gate' must be present in verify allowlist.")
            }
        }
    }

    if (-not $hasConfirmation -or $null -eq $Policy.confirmation) {
        $errors.Add("Missing 'confirmation' object.")
    }
    else {
        if ($null -eq $Policy.confirmation.enabled) {
            $errors.Add("Missing 'confirmation.enabled' field.")
        }
        if ([string]::IsNullOrWhiteSpace([string]$Policy.confirmation.mode)) {
            $errors.Add("Missing 'confirmation.mode' field.")
        }
        elseif ($Policy.confirmation.mode -notin $script:SupportedConfirmationModes) {
            $errors.Add("Unsupported confirmation mode '$($Policy.confirmation.mode)'.")
        }

        if ($Policy.confirmation.enabled -eq $false) {
            $warnings.Add("Confirmation is disabled. Gstack invocations will run without explicit user confirmation.")
        }
    }

    return [pscustomobject]@{
        IsValid  = ($errors.Count -eq 0)
        Errors   = @($errors)
        Warnings = @($warnings)
    }
}

function Test-OpsxGstackPresetsObject {
    param(
        [Parameter(Mandatory = $true)]
        [pscustomobject] $Presets
    )

    $errors = New-Object System.Collections.Generic.List[string]
    $warnings = New-Object System.Collections.Generic.List[string]

    if ($null -eq $Presets.presets) {
        $errors.Add("Missing 'presets' object.")
        return [pscustomobject]@{
            IsValid  = $false
            Errors   = @($errors)
            Warnings = @($warnings)
        }
    }

    foreach ($presetName in $Presets.presets.PSObject.Properties.Name) {
        $preset = $Presets.presets.$presetName
        if ($null -eq $preset.mode -or $preset.mode -notin $script:OpsxModes) {
            $errors.Add("Preset '$presetName' has invalid mode '$($preset.mode)'.")
        }
        if ($null -eq $preset.allowlist) {
            $errors.Add("Preset '$presetName' is missing allowlist.")
            continue
        }

        if ($null -ne $preset.confirmation) {
            if ([string]::IsNullOrWhiteSpace([string]$preset.confirmation.mode)) {
                $errors.Add("Preset '$presetName' has missing confirmation mode.")
            }
            elseif ($preset.confirmation.mode -notin $script:SupportedConfirmationModes) {
                $errors.Add("Preset '$presetName' has unsupported confirmation mode '$($preset.confirmation.mode)'.")
            }
        }

        foreach ($phase in $script:OpsxPhases) {
            if ($null -eq $preset.allowlist.$phase) {
                $errors.Add("Preset '$presetName' is missing allowlist phase '$phase'.")
                continue
            }
            foreach ($skill in @($preset.allowlist.$phase)) {
                if ($skill -notin $script:KnownSafeSkills) {
                    $errors.Add("Preset '$presetName' phase '$phase' contains unknown/non-safe skill '$skill'.")
                }
            }
        }

        $verifySkills = @($preset.allowlist.verify)
        if ("gstack-browse" -in $verifySkills) {
            $errors.Add("Preset '$presetName' must not include gstack-browse in verify allowlist.")
        }
    }

    return [pscustomobject]@{
        IsValid  = ($errors.Count -eq 0)
        Errors   = @($errors)
        Warnings = @($warnings)
    }
}

function Get-OpsxGstackVerifyVerdict {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("off", "assist", "strict")]
        [string] $Mode,

        [Parameter(Mandatory = $true)]
        [array] $GateResults,

        [Parameter(Mandatory = $false)]
        [string[]] $RequiredVerifyGates = @()
    )

    $blockingReasons = New-Object System.Collections.Generic.List[string]

    if ($Mode -eq "strict") {
        foreach ($requiredGate in $RequiredVerifyGates) {
            $matches = @($GateResults | Where-Object { $_.Skill -eq $requiredGate })
            if ($matches.Count -eq 0) {
                $blockingReasons.Add("Missing required strict gate result for '$requiredGate'.")
                continue
            }

            foreach ($gate in $matches) {
                if ($gate.Status -eq "fail" -and $gate.Severity -eq "critical") {
                    $blockingReasons.Add("Strict gate '$requiredGate' failed with critical severity.")
                }
            }
        }
    }

    return [pscustomobject]@{
        Ready           = ($blockingReasons.Count -eq 0)
        BlockingReasons = @($blockingReasons)
    }
}
