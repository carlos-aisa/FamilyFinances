## ADDED Requirements

### Requirement: Repository SHALL Provide a Dedicated CI Quality Workflow
The repository MUST provide a dedicated quality workflow that validates build and test health independently from release packaging concerns.

#### Scenario: Quality workflow runs for pull requests to main
- **WHEN** a pull request targets `main`
- **THEN** the quality workflow MUST execute restore, build, and test validation
- **AND** the workflow MUST report a named check status visible in the pull request checks panel

#### Scenario: Quality workflow runs for direct pushes to integration branches
- **WHEN** changes are pushed directly to `main` or `develop`
- **THEN** the quality workflow MUST execute restore, build, and test validation
- **AND** the workflow MUST NOT perform Windows release packaging or release publication

### Requirement: CI Quality Workflow SHALL Publish Test and Coverage Evidence
The quality workflow MUST produce deterministic test and coverage outputs so reviewers can inspect quality signals from GitHub checks.

#### Scenario: Test results and coverage artifacts are published for review
- **WHEN** the quality workflow test step completes
- **THEN** test result files (`.trx`) MUST be published as workflow artifacts
- **AND** coverage result files (`coverage.cobertura.xml`) MUST be published as workflow artifacts

#### Scenario: Coverage collection uses native .NET collector in CI
- **WHEN** tests run in the quality workflow
- **THEN** the workflow MUST enable XPlat code coverage collection
- **AND** coverage collection MUST work without requiring external SaaS credentials

### Requirement: Branch Governance SHALL Enforce Required Checks on Main Only
The repository branch policy MUST enforce merge-blocking quality checks for `main` while keeping `develop` checks informational.

#### Scenario: Main branch blocks merge when required quality/security checks are failing
- **WHEN** a pull request targets `main` and required checks are failing or pending
- **THEN** merge MUST remain blocked until required checks pass

#### Scenario: Develop branch keeps quality/security checks informational on push
- **WHEN** changes are pushed to `develop`
- **THEN** quality/security checks configured for branch push validation MUST run and report status
- **AND** those checks MUST NOT be required merge blockers for `develop` in this change

### Requirement: Quality Check Identity SHALL Be Stable for Branch Protection
Check names used by branch protection MUST remain deterministic across workflow runs.

#### Scenario: Required check names remain stable
- **WHEN** branch protection is configured for `main`
- **THEN** required check identifiers MUST match stable workflow/job names
- **AND** those identifiers MUST not depend on dynamic naming patterns that change between runs
