## ADDED Requirements

### Requirement: OpenSpec SHALL Preserve Artifact-First Governance Under GStack Integration
OpenSpec command flows MUST preserve artifact-first governance even when gstack-assisted orchestration is enabled.

#### Scenario: Explore/apply/verify continue to rely on OpenSpec artifacts
- **WHEN** a user runs `opsx:explore`, `opsx:apply`, or `opsx:verify` with gstack integration enabled
- **THEN** OpenSpec MUST continue to use change artifacts (`proposal`, `specs`, `design`, `tasks`) as the source of truth for scope and completion
- **AND** gstack outputs MUST be treated as advisory evidence unless explicitly reflected in OpenSpec artifacts

#### Scenario: Integration does not alter archive completion semantics
- **WHEN** a user reaches `opsx:archive`
- **THEN** completion MUST still be determined by OpenSpec completion criteria
- **AND** gstack evidence MUST not bypass incomplete OpenSpec tasks or missing required artifacts

### Requirement: `opsx:explore` SHALL Support Optional GStack-Assisted Specification Challenge
`opsx:explore` MUST support optional invocation of selected gstack planning/review skills to strengthen requirement and architecture exploration.

#### Scenario: Explore runs configured gstack challenge set in assist/strict modes
- **WHEN** integration mode is `assist` or `strict` and `opsx:explore` phase mappings are configured
- **THEN** `opsx:explore` MUST invoke only configured explore-safe skills from the allowed set
- **AND** it MUST summarize findings as decisions/questions linked to the active change context

#### Scenario: Explore gracefully degrades when gstack is unavailable
- **WHEN** gstack binaries/skills are unavailable or fail to execute
- **THEN** `opsx:explore` MUST continue in OpenSpec-only mode
- **AND** it MUST surface a non-fatal warning with the failed step and fallback behavior

### Requirement: `opsx:apply` SHALL Support Policy-Driven Quality Checkpoints
`opsx:apply` MUST support optional policy-driven checkpoints for review and QA while implementing OpenSpec tasks.

#### Scenario: Apply executes non-destructive checkpoint skills
- **WHEN** integration mode enables apply checkpoints
- **THEN** `opsx:apply` MUST invoke only allowlisted non-destructive skills for review/test validation
- **AND** checkpoint results MUST be captured as implementation evidence for the current change

#### Scenario: Apply forbids release/deploy skill invocation
- **WHEN** `opsx:apply` runs under any integration mode
- **THEN** it MUST NOT invoke release/deploy skills (including `ship` and `land-and-deploy`)
- **AND** it MUST report policy violation if such invocation is requested via mapping or override

### Requirement: `opsx:verify` SHALL Produce Unified OpenSpec+GStack Verification Evidence
`opsx:verify` MUST produce a unified verification summary combining OpenSpec checks and optional gstack evidence.

#### Scenario: Verify includes combined scorecard and per-gate verdict
- **WHEN** `opsx:verify` runs with integration enabled
- **THEN** the output MUST include OpenSpec completeness/correctness/coherence findings
- **AND** it MUST include gstack checkpoint outcomes with pass/warn/fail status per gate

#### Scenario: Strict mode blocks ready verdict on critical gstack gate failure
- **WHEN** mode is `strict` and a configured verification gate fails with critical severity
- **THEN** `opsx:verify` MUST return a non-ready verdict
- **AND** it MUST provide explicit remediation actions before archive can proceed

### Requirement: Integration Configuration SHALL Be Explicit, Safe, and Default-Off
Gstack orchestration in OpenSpec MUST be controlled by explicit configuration and default to disabled behavior.

#### Scenario: Missing configuration keeps baseline behavior
- **WHEN** no integration configuration is present
- **THEN** all `opsx:*` commands MUST run baseline OpenSpec behavior
- **AND** no gstack skill invocation MUST occur automatically

#### Scenario: Configuration enforces safe allowlist
- **WHEN** integration configuration is loaded
- **THEN** only phase-allowed and safety-allowlisted skills MUST be eligible for invocation
- **AND** unknown or disallowed skills MUST be rejected with a deterministic warning

### Requirement: Default Phase Allowlist SHALL Be Deterministic
The integration MUST define and enforce a deterministic default allowlist per OpenSpec phase.

#### Scenario: Explore uses only default explore allowlist
- **WHEN** integration mode is enabled and no custom phase override is provided for `explore`
- **THEN** `opsx:explore` MUST only invoke `gstack-office-hours`, `gstack-plan-ceo-review`, `gstack-plan-eng-review`, and `gstack-plan-design-review`
- **AND** any additional skill request MUST be rejected as policy violation

#### Scenario: Apply uses only default apply allowlist
- **WHEN** integration mode is enabled and no custom phase override is provided for `apply`
- **THEN** `opsx:apply` MUST only invoke `gstack-review`, `gstack-qa`, `gstack-qa-only`, `gstack-design-review`, `gstack-cso`, `gstack-investigate`, and `gstack-browse`
- **AND** any additional skill request MUST be rejected as policy violation

#### Scenario: Verify uses only default verify allowlist
- **WHEN** integration mode is enabled and no custom phase override is provided for `verify`
- **THEN** `opsx:verify` MUST only invoke `gstack-review`, `gstack-qa-only`, `gstack-cso`, and `gstack-benchmark`
- **AND** any additional skill request MUST be rejected as policy violation

### Requirement: Release/Deploy Skills SHALL Be Hard-Blocked In All OpenSpec Phases
OpenSpec orchestration MUST hard-block release/deploy gstack skills in every phase.

#### Scenario: Hard-blocked release/deploy skills are rejected
- **WHEN** mapping or override includes `gstack-ship`, `gstack-land-and-deploy`, or `gstack-setup-deploy`
- **THEN** the invocation MUST be denied before execution
- **AND** `opsx:*` MUST report a deterministic policy error naming the blocked skill

### Requirement: Browse Profile SHALL Stay Advisory And Apply-Only
The browse profile MUST be constrained to advisory UI diagnostics during apply flow.

#### Scenario: Browse is accepted only in apply phase under assist semantics
- **WHEN** `gstack-browse` is requested by policy or user intent
- **THEN** `opsx:apply` MAY execute it only as advisory evidence
- **AND** strict-mode readiness decisions MUST NOT require browse outcomes

#### Scenario: Browse is ignored for verify blocking logic
- **WHEN** verify evidence contains browse outcomes
- **THEN** `opsx:verify` MUST report them as informational/advisory only
- **AND** they MUST NOT act as strict blocking gates

### Requirement: GStack Invocation SHALL Require User Notification And Confirmation
OpenSpec orchestration MUST notify the user before running each gstack skill and MUST support explicit confirmation-first behavior.

#### Scenario: Confirmation-first invocation runs only after user approval
- **WHEN** policy `confirmation.enabled=true` and `confirmation.mode=ask-per-invocation`
- **THEN** each gstack skill invocation MUST be preceded by a user confirmation prompt
- **AND** the skill MUST execute only after explicit user approval

#### Scenario: User-declined invocation does not block baseline OpenSpec flow
- **WHEN** user declines a prompted gstack invocation
- **THEN** the invocation MUST be marked as skipped-by-user
- **AND** OpenSpec flow MUST continue with deterministic baseline/advisory behavior

#### Scenario: Notify-only mode does not require interactive confirmation
- **WHEN** policy `confirmation.mode=notify-only`
- **THEN** OpenSpec MUST notify the user before gstack invocation
- **AND** invocation MAY proceed without explicit confirmation
