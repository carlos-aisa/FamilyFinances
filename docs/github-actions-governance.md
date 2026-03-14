# GitHub Actions Governance

This document defines the workflow ownership model, merge-gate policy, and operational guardrails for this repository.

## Workflow Ownership Model

The repository uses a split workflow model:

- `ci-quality.yml`
  - Purpose: build, test, and publish test/coverage evidence.
  - Trigger: PR (`main`) and push (`main`, `develop`).
- `dependency-review.yml`
  - Purpose: dependency risk check for incoming PR changes.
  - Trigger: PR (`main`) only.
- `codeql.yml`
  - Purpose: C# code scanning for security and code risk.
  - Trigger: PR (`main`), push (`main`, `develop`), weekly schedule.
- `release-windows.yml`
  - Purpose: tag-driven Windows ZIP packaging and release publishing.
  - Trigger: push tags `v*.*.*` and optional manual ZIP cleanup via `workflow_dispatch`.

## Branch Protection Policy

### Main branch
- Required checks:
  - `ci-quality`
  - `dependency-review`
  - `analyze` (CodeQL)

### Develop branch
- Checks run on branch pushes for visibility and signal.
- Checks are informational and not required blockers in this phase.

## Storage and Retention Policy

- Windows release packaging is tag-only to avoid unnecessary ZIP generation on branch pushes.
- Release ZIP cleanup runs before publish in `release-windows.yml`.
- ZIP retention policy keeps only 2 historical matching ZIP assets (`FamilyFinances-v*-win-x64.zip`).
- Workflow artifact retention for release ZIP runs is intentionally low.

## Coverage Visibility Policy

Current policy (no external SaaS dependency):
- Publish `coverage.cobertura.xml` as Actions artifacts from `ci-quality`.
- Publish test TRX files as Actions artifacts from `ci-quality`.
- Add run summary guidance to point reviewers to artifacts.

Future enhancement (out of current scope):
- Evaluate Codecov or Coveralls for richer UI ("coverage bonito"), PR diff coverage, and historical trends.
- If adopted, document token/permissions model and required status check behavior before rollout.

## Operational Checklist

- Ensure workflow/job names remain stable before changing branch protection rules.
- Validate workflow updates through PR checks before release tagging.
- Keep permissions minimal per job (`contents: read` by default, write only where needed).
- Avoid duplicate release publish paths across workflows/events.
