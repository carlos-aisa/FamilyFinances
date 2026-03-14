# GitHub Actions Governance

This document defines the workflow ownership model, merge-gate policy, and operational guardrails for this repository.

## Workflow Ownership Model

The repository uses a split workflow model:

- `ci-quality.yml`
  - Purpose: build, test, and publish coverage summary evidence in-run.
  - Trigger: PR (`main`) and push (`main`, `develop`).
  - Note: PR runs also attempt raw TRX/coverage artifact upload as best effort.
- `dependency-review.yml`
  - Purpose: dependency risk check for incoming PR changes.
  - Trigger: PR (`main`) only.
  - Note: auto-skips on private repositories where Code Security/GHAS is not enabled.
- `codeql.yml`
  - Purpose: C# code scanning for security and code risk.
  - Trigger: PR (`main`), push (`main`, `develop`), weekly schedule.
  - Note: auto-skips on private repositories where Code Security/GHAS is not enabled.
- `release-windows.yml`
  - Purpose: tag-driven Windows ZIP packaging and release publishing.
  - Trigger: push tags `v*.*.*` and optional manual ZIP cleanup via `workflow_dispatch`.
- `actions-artifacts-cleanup.yml`
  - Purpose: delete old Actions artifacts to control storage growth.
  - Trigger: daily schedule and optional manual cleanup via `workflow_dispatch`.

## Branch Protection Policy

### Main branch
- Required checks (public repo with Code Security/GHAS):
  - `ci-quality`
  - `dependency-review`
  - `analyze` (CodeQL)
- Required checks (private repo without Code Security/GHAS):
  - `ci-quality`

### Develop branch
- Checks run on branch pushes for visibility and signal.
- Checks are informational and not required blockers in this phase.

## Storage and Retention Policy

- Windows release packaging is tag-only to avoid unnecessary ZIP generation on branch pushes.
- Release ZIP cleanup runs before publish in `release-windows.yml`.
- ZIP retention policy keeps only 2 historical matching ZIP assets (`FamilyFinances-v*-win-x64.zip`).
- Release ZIP is attached directly to GitHub Release (no duplicate Actions artifact upload).
- CI artifact retention is intentionally low and PR-scoped for test/coverage evidence.
- `actions-artifacts-cleanup.yml` performs scheduled/manual cleanup of old Actions artifacts.

## Coverage Visibility Policy

Current policy (no external SaaS dependency):
- Publish coverage summary directly in `ci-quality` run summary.
- On PR runs, attempt short-lived TRX + raw coverage artifact upload as best effort.

Future enhancement (out of current scope):
- Evaluate Codecov or Coveralls for richer UI ("coverage bonito"), PR diff coverage, and historical trends.
- If adopted, document token/permissions model and required status check behavior before rollout.

## Operational Checklist

- Ensure workflow/job names remain stable before changing branch protection rules.
- Validate workflow updates through PR checks before release tagging.
- Keep permissions minimal per job (`contents: read` by default, write only where needed).
- Avoid duplicate release publish paths across workflows/events.
- Revisit required checks when repository visibility/licensing changes (private/public, GHAS enabled/disabled).
