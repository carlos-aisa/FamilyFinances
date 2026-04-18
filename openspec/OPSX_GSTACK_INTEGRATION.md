# OpenSpec + GStack Integration Guide

## Purpose

This document defines how `opsx:*` workflows can use gstack safely without replacing OpenSpec artifact governance.

OpenSpec remains the source of truth for:
- scope,
- requirements,
- design decisions,
- task completion,
- archive readiness.

gstack is an optional quality layer for challenge/review/QA evidence.

## Activation

Integration is controlled by:
- `.codex/opsx-gstack-policy.json`
- optional presets in `.codex/opsx-gstack-policy.presets.json`

If the policy file is missing, `opsx:*` commands run baseline OpenSpec behavior.

## User Confirmation Before Invocation

By default, gstack invocation requires user confirmation before each execution.

Policy fields:
- `confirmation.enabled`
- `confirmation.mode` (`ask-per-invocation` or `notify-only`)
- optional `confirmation.promptTemplate`

Recommended default:
- `enabled: true`
- `mode: ask-per-invocation`

Behavior:
- ask-per-invocation: notify + ask confirmation each time.
- notify-only: notify user but do not block on confirmation.
- If user declines in ask mode, skip that checkpoint and continue OpenSpec flow.

## Modes

- `off`
  - OpenSpec only.
  - No gstack orchestration.
- `assist`
  - Run allowlisted gstack skills as advisory checkpoints.
  - Findings do not block readiness by themselves.
- `strict`
  - Run allowlisted gstack skills.
  - Critical failures on required strict gates block readiness in verify.

## Safe Mappings (Default Allowlist)

### Explore (`opsx:explore`)
- `gstack-office-hours`
- `gstack-plan-ceo-review`
- `gstack-plan-eng-review`
- `gstack-plan-design-review`

### Apply (`opsx:apply`)
- `gstack-review`
- `gstack-qa`
- `gstack-qa-only`
- `gstack-design-review`
- `gstack-cso`
- `gstack-investigate`
- `gstack-browse` (assist-only, explicit request, advisory only)

### Verify (`opsx:verify`)
- `gstack-review`
- `gstack-qa-only`
- `gstack-cso`
- `gstack-benchmark`

## Hard Blocklist (All Phases)

- `gstack-ship`
- `gstack-land-and-deploy`
- `gstack-setup-deploy`

These skills must never be invoked by `opsx:*` orchestration.

## Browse Guidance

`gstack-browse` is intended for UI diagnostics and automation assistance in apply phase.

Rules:
- use only in `opsx:apply`,
- use only in `assist` mode,
- require explicit user intent,
- keep results advisory,
- do not treat browse output as a strict verify gate.

## Evidence Contract

When gstack checkpoints run, record evidence at:
- `openspec/changes/<change-name>/gstack-evidence.md`

Suggested sections:
- mode and policy snapshot,
- executed skills and timestamps,
- pass/warn/fail outcomes,
- severity labels,
- remediation actions,
- links to logs/screenshots where available.

## Fallback Behavior

If gstack is unavailable or a skill invocation fails:
- keep running OpenSpec baseline flow,
- emit a non-fatal warning,
- continue implementation/verification.

Integration must never block baseline OpenSpec usage due to local tooling absence.

## Rollout Strategy

1. Start with `assist` mode on selected changes.
2. Validate evidence quality and signal-to-noise.
3. Promote specific change classes to `strict` only when gates are stable.
4. Keep policy narrow and explicit.

## Troubleshooting

### Missing gstack skill
- Verify local `.agents/skills/gstack-*` installation.
- Re-run gstack setup if required.
- Continue in baseline mode if missing.

### Policy validation errors
- Run:
```powershell
powershell -ExecutionPolicy Bypass -File .\tools\opsx\validate-opsx-gstack-policy.ps1
```
- Fix malformed mode/allowlist/blocklist values.

### Strict mode blocks verify unexpectedly
- Inspect required strict gates in `.codex/opsx-gstack-policy.json`.
- Ensure evidence includes those gates with non-critical-fail outcomes.
- Downgrade to `assist` temporarily if gate reliability is under investigation.

## Maintenance

- Revalidate policy after gstack upgrades.
- Keep allowlist conservative; expand only with explicit review.
- Preserve blocklist invariants.
- Re-run policy validation and test scripts after any policy/schema changes:
```powershell
powershell -ExecutionPolicy Bypass -File .\tools\opsx\validate-opsx-gstack-policy.ps1
powershell -ExecutionPolicy Bypass -File .\tools\opsx\test-opsx-gstack-policy.ps1
powershell -ExecutionPolicy Bypass -File .\tools\opsx\dry-run-opsx-gstack-integration.ps1 -Mode off
powershell -ExecutionPolicy Bypass -File .\tools\opsx\dry-run-opsx-gstack-integration.ps1 -Mode assist
powershell -ExecutionPolicy Bypass -File .\tools\opsx\dry-run-opsx-gstack-integration.ps1 -Mode strict
```
