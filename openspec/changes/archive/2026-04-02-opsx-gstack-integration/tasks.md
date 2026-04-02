## 1. Integration Policy Contract

- [x] 1.1 Define project-local OpenSpec+gstack policy schema with mode (`off|assist|strict`) and per-phase mappings (`explore|apply|verify`).
- [x] 1.2 Add deterministic defaults (mode `off`) and document fallback behavior when policy is missing.
- [x] 1.3 Implement deterministic default phase allowlist (`explore`: `gstack-office-hours`, `gstack-plan-ceo-review`, `gstack-plan-eng-review`, `gstack-plan-design-review`; `apply`: `gstack-review`, `gstack-qa`, `gstack-qa-only`, `gstack-design-review`, `gstack-cso`, `gstack-investigate`, `gstack-browse`; `verify`: `gstack-review`, `gstack-qa-only`, `gstack-cso`, `gstack-benchmark`).
- [x] 1.4 Implement hard blocklist for `gstack-ship`, `gstack-land-and-deploy`, and `gstack-setup-deploy` across all `opsx:*` phases.
- [x] 1.5 Add sample policy presets for common change profiles (UI-heavy, security-heavy, backend-only).

## 2. `opsx:explore` Orchestration

- [x] 2.1 Update `openspec-explore` instructions to optionally execute explore-safe gstack challenge skills according to policy.
- [x] 2.2 Ensure explore flow remains non-implementing and artifact-aware while incorporating gstack findings.
- [x] 2.3 Add deterministic warning/fallback behavior when gstack commands fail or are unavailable.
- [x] 2.4 Add structured explore-summary format that captures challenge findings as decisions/open questions.

## 3. `opsx:apply` Checkpoints

- [x] 3.1 Update `openspec-apply-change` instructions to run configured non-destructive gstack checkpoints during implementation.
- [x] 3.2 Persist checkpoint outputs into a dedicated change-local evidence artifact.
- [x] 3.3 Enforce policy violation handling for disallowed mappings (including attempted `ship`/`land-and-deploy`).
- [x] 3.4 Keep task checkbox progression semantics unchanged from baseline OpenSpec behavior.

## 4. `opsx:verify` Unified Gates

- [x] 4.1 Update `openspec-verify-change` instructions to read gstack evidence and merge it with OpenSpec completeness/correctness/coherence report.
- [x] 4.2 Implement mode-aware verdict logic (`assist` advisory, `strict` blocking on critical gate failures).
- [x] 4.3 Add output contract for combined scorecard including per-gate status and remediation actions.
- [x] 4.4 Ensure verify remains usable when no gstack evidence exists (baseline output + explicit note).

## 5. Documentation and Operator Guidance

- [x] 5.1 Add integration guidance in OpenSpec docs (activation, modes, safe mappings, fallback behavior).
- [x] 5.2 Document recommended rollout strategy (start in `assist`, graduate selected flows to `strict`).
- [x] 5.3 Document troubleshooting for missing skills, version drift, and policy validation failures.
- [x] 5.4 Add maintenance guidance for updating allowlist when gstack skill set evolves.

## 6. Validation and Test Updates

- [x] 6.1 Add automated validation checks for policy parsing, allowlist enforcement, and fallback paths.
- [x] 6.2 Add deterministic test coverage for strict-mode blocking semantics in verify.
- [x] 6.3 Execute OpenSpec dry runs for `opsx:explore`, `opsx:apply`, and `opsx:verify` in `off`, `assist`, and `strict` modes.
- [x] 6.4 Run repository validation checks (`openspec status --change "opsx-gstack-integration"`, docs consistency checks) and resolve any drift.

## 7. Confirmation-First Pilot Behavior

- [x] 7.1 Add policy contract for user notification/confirmation before each gstack skill invocation (`ask-per-invocation` and `notify-only`).
- [x] 7.2 Update `opsx:explore`, `opsx:apply`, and `opsx:verify` skill instructions to require pre-invocation confirmation in ask mode and skip-on-decline behavior.
- [x] 7.3 Extend policy validation/tests and documentation to cover confirmation mode behavior.
