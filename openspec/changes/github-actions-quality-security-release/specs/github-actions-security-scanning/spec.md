## ADDED Requirements

### Requirement: Pull Requests SHALL Be Evaluated by Dependency Review
The repository MUST run dependency risk analysis for pull requests so newly introduced vulnerable dependency changes are detected before merge.

#### Scenario: Dependency review runs for pull requests to main
- **WHEN** a pull request targets `main`
- **THEN** a dependency review check MUST execute against the dependency diff in that pull request
- **AND** the check result MUST be visible in the pull request checks panel

#### Scenario: Dependency review remains pull-request scoped
- **WHEN** code is pushed without a pull request context
- **THEN** dependency review MUST NOT run as a branch-push-only workflow

### Requirement: Repository SHALL Run CodeQL for CSharp Sources
The repository MUST run CodeQL scanning for C# code to provide continuous code scanning coverage in PR, push, and scheduled contexts.

#### Scenario: CodeQL runs on pull requests to main and integration branch pushes
- **WHEN** a pull request targets `main`, or code is pushed to `main` or `develop`
- **THEN** CodeQL analysis for `csharp` MUST execute
- **AND** scan results MUST be published to repository code scanning results

#### Scenario: CodeQL runs on a periodic schedule
- **WHEN** the weekly CodeQL schedule trigger fires
- **THEN** CodeQL analysis for `csharp` MUST execute even if no pull request is open

### Requirement: Security Scanning Workflows SHALL Use Minimum Required Permissions
Security workflows MUST use least-privilege permissions while preserving required publishing behavior.

#### Scenario: Dependency review uses read-scoped permissions
- **WHEN** dependency review workflow executes
- **THEN** workflow permissions MUST be limited to read access required for dependency diff inspection

#### Scenario: CodeQL grants security event write for analysis publication
- **WHEN** CodeQL workflow executes
- **THEN** workflow permissions MUST include `security-events: write` for result publication
- **AND** permissions unrelated to scanning MUST remain read-only unless explicitly required
