# github-actions-security-scanning Specification

## Purpose
Define repository-level security scanning requirements for pull requests, integration branch pushes, and scheduled checks in GitHub Actions, including graceful behavior when repository security features are unavailable.

## Requirements
### Requirement: Pull Requests SHALL Be Evaluated by Dependency Review
The repository MUST run dependency risk analysis for pull requests so newly introduced vulnerable dependency changes are detected before merge when repository security capabilities are available.

#### Scenario: Dependency review runs for pull requests to main when security features are available
- **WHEN** a pull request targets `main` and repository security features required by dependency review are available
- **THEN** a dependency review check MUST execute against the dependency diff in that pull request
- **AND** the check result MUST be visible in the pull request checks panel

#### Scenario: Dependency review remains pull-request scoped
- **WHEN** code is pushed without a pull request context
- **THEN** dependency review MUST NOT run as a branch-push-only workflow

#### Scenario: Dependency review degrades gracefully when repository security features are unavailable
- **WHEN** a pull request targets `main` but repository security features required by dependency review are unavailable (for example private repository without GHAS)
- **THEN** the workflow MAY skip dependency review execution
- **AND** the workflow MUST NOT fail solely due to missing repository security feature availability

### Requirement: Repository SHALL Run CodeQL for CSharp Sources
The repository MUST run CodeQL scanning for C# code to provide continuous code scanning coverage in PR, push, and scheduled contexts when code scanning is enabled for the repository.

#### Scenario: CodeQL runs on pull requests to main and integration branch pushes when code scanning is enabled
- **WHEN** a pull request targets `main`, or code is pushed to `main` or `develop`, and code scanning is enabled for the repository
- **THEN** CodeQL analysis for `csharp` MUST execute
- **AND** scan results MUST be published to repository code scanning results

#### Scenario: CodeQL runs on a periodic schedule when code scanning is enabled
- **WHEN** the weekly CodeQL schedule trigger fires and code scanning is enabled for the repository
- **THEN** CodeQL analysis for `csharp` MUST execute even if no pull request is open

#### Scenario: CodeQL degrades gracefully when code scanning is unavailable
- **WHEN** CodeQL workflow triggers but code scanning is unavailable for the repository (for example private repository without GHAS)
- **THEN** the workflow MAY skip CodeQL analysis execution
- **AND** the workflow MUST NOT fail solely due to missing code scanning feature availability

### Requirement: Security Scanning Workflows SHALL Use Minimum Required Permissions
Security workflows MUST use least-privilege permissions while preserving required publishing behavior.

#### Scenario: Dependency review uses read-scoped permissions
- **WHEN** dependency review workflow executes
- **THEN** workflow permissions MUST be limited to read access required for dependency diff inspection

#### Scenario: CodeQL grants security event write for analysis publication
- **WHEN** CodeQL workflow executes
- **THEN** workflow permissions MUST include `security-events: write` for result publication
- **AND** permissions unrelated to scanning MUST remain read-only unless explicitly required
