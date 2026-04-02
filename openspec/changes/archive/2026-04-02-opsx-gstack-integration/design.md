## Context

Current OpenSpec skills in this repository provide strong lifecycle control for `explore -> apply -> verify -> archive` and are already used as the team default. gstack is now available locally and brings high-value specialist workflows (challenge review, code review, browser QA), but with a much broader command surface that includes destructive/release operations not appropriate for automatic invocation from OpenSpec flows.

The design goal is to integrate gstack as a quality amplifier while preserving OpenSpec ownership and predictable behavior. Integration must remain optional and safe-by-default because not all contributors will have gstack installed.

Constraints:
- OpenSpec artifact governance cannot be bypassed.
- No runtime app behavior/API changes are in scope.
- Existing `opsx:*` commands must keep working when gstack is absent.
- Integration must be policy-driven, not ad-hoc command chaining.

## Goals / Non-Goals

**Goals:**
- Add a clear, deterministic orchestration contract for using selected gstack skills inside `opsx:explore`, `opsx:apply`, and `opsx:verify`.
- Introduce explicit integration modes (`off`, `assist`, `strict`) to control enforcement strength.
- Define a per-phase safe allowlist so OpenSpec never invokes dangerous release/deploy commands.
- Standardize how gstack findings are captured and surfaced in verification results.
- Keep integration implementation localized to OpenSpec skill instructions/configuration/docs.

**Non-Goals:**
- Replacing OpenSpec workflows with gstack workflows.
- Auto-shipping/deployment from OpenSpec commands.
- Implementing cross-model orchestration outside the OpenSpec command scope.
- Building a generic plugin framework for arbitrary third-party toolchains.

## Decisions

### Decision 1: Policy-first integration contract
- **Choice:** Introduce an explicit integration policy file (project-local) that declares mode and per-phase skill mappings.
- **Rationale:** Keeps behavior deterministic, auditable, and easy to disable.
- **Alternative considered:** hardcode mappings in each OpenSpec skill file.
  - **Rejected because:** brittle and difficult to tune per repository/change type.

### Decision 2: Safety allowlist enforced at orchestration boundary
- **Choice:** Define a deterministic phase allowlist of gstack skills eligible for OpenSpec orchestration.
  - `explore`: `gstack-office-hours`, `gstack-plan-ceo-review`, `gstack-plan-eng-review`, `gstack-plan-design-review`
  - `apply`: `gstack-review`, `gstack-qa`, `gstack-qa-only`, `gstack-design-review`, `gstack-cso`, `gstack-investigate`, `gstack-browse`
  - `verify`: `gstack-review`, `gstack-qa-only`, `gstack-cso`, `gstack-benchmark`
  - Hard-blocked in all phases: `gstack-ship`, `gstack-land-and-deploy`, `gstack-setup-deploy`
  - `gstack-browse` is advisory-only and limited to apply-phase UI diagnostics.
- **Rationale:** Prevent accidental high-impact actions while still gaining review/QA value.
- **Alternative considered:** trust user prompts without allowlist enforcement.
  - **Rejected because:** prompt drift can cause unsafe invocation.

### Decision 3: Mode semantics (`off`, `assist`, `strict`)
- **Choice:** implement three deterministic integration modes.
  - `off`: OpenSpec-only behavior.
  - `assist`: run configured checkpoints, findings are advisory.
  - `strict`: configured critical gate failures block ready verdict in verify.
- **Rationale:** Teams can adopt gradually without immediate process friction.
- **Alternative considered:** single integration mode.
  - **Rejected because:** too rigid for staged rollout.

### Decision 4: Unified evidence artifact for verify
- **Choice:** persist orchestration outcomes to a dedicated change-local evidence file (for example `openspec/changes/<change>/gstack-verification.md`) and include its summary in `opsx:verify` output.
- **Rationale:** keeps evidence discoverable and separates operational findings from normative specs/tasks.
- **Alternative considered:** write findings directly into `tasks.md`.
  - **Rejected because:** mixes implementation checklist state with external verification telemetry.

### Decision 5: Graceful degradation path is mandatory
- **Choice:** any failure to load/execute gstack skills falls back to baseline OpenSpec behavior with explicit warning output.
- **Rationale:** preserves contributor productivity and avoids hard dependency on local tooling setup.
- **Alternative considered:** fail command when gstack is unavailable.
  - **Rejected because:** would break OpenSpec for contributors without gstack.

### Decision 6: Confirmation-first invocation policy
- **Choice:** require user notification and explicit confirmation before each gstack invocation by default (`confirmation.mode=ask-per-invocation`), with optional `notify-only` mode.
- **Rationale:** improves operator trust and gives precise control while piloting orchestration behavior.
- **Alternative considered:** silent orchestration under allowlist-only controls.
  - **Rejected because:** less transparent and harder to validate during early rollout.

## Risks / Trade-offs

- [Risk] Integration rules become too complex for contributors → Mitigation: keep one small policy schema and provide documented presets.
- [Risk] False negatives/positives in strict mode gates → Mitigation: classify gate severities and allow per-phase override in policy.
- [Risk] Prompt/context bloat from extra skill orchestration → Mitigation: store detailed outputs in evidence artifacts and summarize in verify.
- [Risk] Skill name/version drift after gstack upgrades → Mitigation: validate configured mappings against allowlist at runtime and warn deterministically.
- [Trade-off] Added process overhead in apply/verify → Mitigation: default `off`, recommended rollout starts with `assist` on selected changes.

## Migration Plan

1. Define and document policy schema and defaults (default mode `off`).
2. Update `openspec-explore` skill to read policy and run explore-safe orchestration when enabled.
3. Update `openspec-apply-change` skill to run apply checkpoints and persist evidence.
4. Update `openspec-verify-change` skill to merge OpenSpec findings with gstack evidence and enforce mode semantics.
5. Add examples/presets for typical change classes (UI-heavy, security-heavy, backend-only).
6. Add validation script/tests for:
   - policy parsing,
   - allowlist enforcement,
   - fallback behavior when gstack is unavailable,
   - strict-mode blocking semantics.
7. Update OpenSpec docs with operator guidance, confirmation behavior, and rollout recommendations.

### Rollback Strategy

1. Set integration mode to `off` in policy (or remove policy file) to immediately restore baseline behavior.
2. Revert OpenSpec skill-file orchestration additions if policy-off is insufficient.
3. Keep evidence files for audit history but exclude them from verify gates after rollback.
4. Run `opsx:verify` in baseline mode to confirm no process regressions.

## Open Questions

- Should strict-mode blocking apply globally or only to selected change tags/types?
- Should `gstack-qa` be optional in `assist` mode for non-UI changes?
- Where should default policy live long-term: repository-local `.codex/` vs OpenSpec-managed config?
