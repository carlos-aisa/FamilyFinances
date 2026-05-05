param(
    [Parameter(Mandatory = $false)]
    [string]$Owner = "carlos-aisa",

    [Parameter(Mandatory = $false)]
    [string]$Repo = "FamilyFinances"
)

$ErrorActionPreference = "Stop"

$requiredLabels = @(
    @{ name = "domain"; color = "0052CC"; description = "Domain model and business invariants" },
    @{ name = "application"; color = "0E8A16"; description = "Application layer orchestration and use cases" },
    @{ name = "business-rules"; color = "5319E7"; description = "Accounting and business rule behavior" },
    @{ name = "api"; color = "1D76DB"; description = "REST API and endpoint changes" },
    @{ name = "integration"; color = "0366D6"; description = "Integration points and external dependencies" },
    @{ name = "web"; color = "FBCA04"; description = "Web UI behavior and components" },
    @{ name = "frontend"; color = "BFD4F2"; description = "Frontend implementation details" },
    @{ name = "ux"; color = "F9D0C4"; description = "User experience and interaction changes" },
    @{ name = "test"; color = "C2E0C6"; description = "Test coverage and test infrastructure" },
    @{ name = "quality"; color = "7057FF"; description = "Quality gates and reliability improvements" },
    @{ name = "ci"; color = "0E8A16"; description = "Continuous integration and pipeline changes" },
    @{ name = "security"; color = "B60205"; description = "Security controls and vulnerability remediation" },
    @{ name = "docs"; color = "0075CA"; description = "Documentation updates" },
    @{ name = "skip-changelog"; color = "EEEEEE"; description = "Exclude from generated release notes" }
)

Write-Host "Fetching existing labels for $Owner/$Repo..."
$existing = gh api "repos/$Owner/$Repo/labels" --paginate | ConvertFrom-Json
$existingByName = @{}
foreach ($label in $existing) {
    $existingByName[$label.name] = $label
}

foreach ($label in $requiredLabels) {
    $name = $label.name
    $payload = @{ color = $label.color; description = $label.description } | ConvertTo-Json -Compress

    if ($existingByName.ContainsKey($name)) {
        Write-Host "Updating label '$name'"
        $payload | gh api "repos/$Owner/$Repo/labels/$name" --method PATCH --input - | Out-Null
    }
    else {
        Write-Host "Creating label '$name'"
        $createPayload = @{ name = $name; color = $label.color; description = $label.description } | ConvertTo-Json -Compress
        $createPayload | gh api "repos/$Owner/$Repo/labels" --method POST --input - | Out-Null
    }
}

Write-Host "Label synchronization completed."
