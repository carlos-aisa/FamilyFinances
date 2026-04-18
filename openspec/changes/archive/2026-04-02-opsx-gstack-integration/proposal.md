## CRITICAL IMPLEMENTATION CONSTRAINTS

### Forbidden
- Do not replace OpenSpec artifact governance (`proposal/specs/design/tasks`) with gstack outputs.
- Do not make gstack mandatory for all projects by default.
- Do not auto-run destructive/release commands (`ship`, `land-and-deploy`) from `opsx:*` workflows.
- Do not break existing `opsx:explore`, `opsx:apply`, and `opsx:verify` behavior when gstack is missing.

### Required
- Keep OpenSpec as the source of truth for scope, requirements, and completion status.
- Integrate gstack through explicit policy mapping per `opsx` command (explore/apply/verify).
- Ensure graceful degradation to current OpenSpec-only behavior when gstack is unavailable.
- Persist gstack findings in structured verification evidence tied to the active change.
- Require user notification and confirmation (test mode) before each gstack skill invocation.

## Why

OpenSpec already provides strong change governance, but it does not natively add cross-model challenge/review loops and browser-driven QA orchestration. Integrating selected gstack skills can improve quality and confidence without sacrificing OpenSpec discipline.

## What Changes

- Introduce an optional orchestration layer that maps gstack skills into OpenSpec command phases:
  - `opsx:explore`: requirement/architecture challenge support.
  - `opsx:apply`: code review and QA checkpoints during implementation.
  - `opsx:verify`: unified evidence and gate decisions before archive.
- Establish the default phase allowlist (policy presets may be stricter, but not broader):
  - `explore`: `gstack-office-hours`, `gstack-plan-ceo-review`, `gstack-plan-eng-review`, `gstack-plan-design-review`
  - `apply`: `gstack-review`, `gstack-qa`, `gstack-qa-only`, `gstack-design-review`, `gstack-cso`, `gstack-investigate`, `gstack-browse`
  - `verify`: `gstack-review`, `gstack-qa-only`, `gstack-cso`, `gstack-benchmark`
- Keep `gstack-browse` explicitly advisory and limited to apply-phase UI diagnostics (assist mode with explicit user intent).
- Establish an explicit blocklist for any `opsx:*` orchestration:
  - `gstack-ship`, `gstack-land-and-deploy`, `gstack-setup-deploy`
- Add explicit policy configuration for integration mode (`off`, `assist`, `strict`) and allowed gstack skills per phase.
- Add explicit policy configuration for pre-invocation confirmation (`ask-per-invocation` or `notify-only`).
- Define safety constraints so `opsx:*` can only invoke a non-destructive allowlist of gstack skills.
- Define evidence contracts for storing gstack findings and final verdicts alongside each change.
- Add documentation and operational guidance for teams on when to use OpenSpec-only vs OpenSpec+gstack.

## Capabilities

### New Capabilities
- `opsx-gstack-orchestration`: Optional and policy-driven orchestration of gstack skills inside OpenSpec `explore/apply/verify` flows with safety gates and evidence capture.

### Modified Capabilities
- `system`: Extend developer-workflow requirements to include safe optional gstack-assisted exploration, implementation checkpoints, and verification gates.

## Impact

- Affected OpenSpec skill definitions:
  - `d:/Programacion/FamilyFinances/.codex/skills/openspec-explore/SKILL.md`
  - `d:/Programacion/FamilyFinances/.codex/skills/openspec-apply-change/SKILL.md`
  - `d:/Programacion/FamilyFinances/.codex/skills/openspec-verify-change/SKILL.md`
- New integration policy/evidence assets under `.codex/` and/or `openspec/changes/<name>/`.
- Documentation updates in OpenSpec guidance files (`openspec/AGENTS.md`, `openspec/project.md`, or dedicated integration guide).
- Validation impact:
  - scripted dry-run checks for policy parsing and fallback paths,
  - verification report assertions for gate behavior and evidence formatting.

## Non-Goals

- Replacing OpenSpec with gstack as the primary workflow.
- Executing release/deploy automation from OpenSpec commands.
- Introducing backend/API/domain changes in FamilyFinances runtime.
- Forcing teams to install gstack to use OpenSpec.

## Rollback Plan

- Keep integration behind an explicit configuration switch (`off` fallback).
- Revert updated OpenSpec skill instructions to current baseline behavior.
- Retain evidence artifacts as read-only history, but ignore them in verification gates after rollback.
- Re-run OpenSpec verification in baseline mode to confirm unchanged archive criteria.
