## CRITICAL IMPLEMENTATION CONSTRAINTS

### Forbidden
- Do not keep Windows ZIP packaging on regular pushes to `main` or `develop`.
- Do not make `develop` merge-blocking with required checks in this change.
- Do not keep release ZIP cleanup only after publish; cleanup must run before publish.
- Do not introduce external coverage SaaS integration (Codecov/Coveralls) in this scope.
- Do not change API contracts, database schema, or business calculations as part of CI/CD updates.

### Required
- Split current mixed CI/CD workflow responsibilities into dedicated workflows for quality, security, and release.
- Enforce quality/security checks as required for `main` and informative for `develop`.
- Add PR dependency risk scanning and repository code scanning with periodic scheduled execution.
- Publish test and coverage outputs in GitHub Actions for PR visibility.
- Run Windows release packaging only from version tags (`v*.*.*`) and keep current packaging verification/smoke checks.
- Clean release ZIP assets before publish, keeping exactly 2 older ZIP assets so the newly published ZIP results in 3 recent assets total.

## Why

The current GitHub Actions pipeline is effective for build and release but combines too many responsibilities and lacks preventive security checks. This change is needed now to reduce release failures caused by storage pressure while adding continuous quality and security guardrails for pull requests.

## What Changes

- Introduce a dedicated quality workflow for PR/push validation with restore, build, tests, and coverage artifact publication.
- Introduce a dedicated dependency review workflow for pull requests targeting `main`.
- Introduce a dedicated CodeQL workflow for pull requests targeting `main`, pushes to `main`/`develop`, and scheduled weekly scanning.
- Move Windows distribution publish flow to a release-only workflow triggered by version tags (`v*.*.*`) instead of regular branch pushes.
- Add pre-publish release ZIP cleanup logic to keep only two prior ZIP assets before publishing a new one.
- Keep existing Windows distribution structural verification and smoke test behavior as release gates.
- Define branch protection policy expectations: required checks on `main`, non-blocking signal checks on `develop`.

## Capabilities

### New Capabilities
- `github-actions-quality-gates`: Defines repository-level quality validation flows for PRs and branch pushes, including test/coverage publication and merge-gating behavior.
- `github-actions-security-scanning`: Defines repository-level security scanning requirements using dependency review and CodeQL with PR/push/scheduled coverage.

### Modified Capabilities
- `windows-distribution-packaging`: Updates CI packaging trigger and retention behavior so Windows ZIP publishing is tag-driven and storage-aware with pre-publish cleanup.

## Impact

- Affected code/config:
  - `.github/workflows/` will gain dedicated workflows (`ci-quality`, `dependency-review`, `codeql`, `release-windows`).
  - Existing `.github/workflows/ci.yml` behavior will be reduced/retired to avoid duplication.
- Affected operational behavior:
  - `main` merge policy depends on required quality/security checks.
  - `develop` continues receiving signal checks without merge blocking.
  - Release publishing frequency and storage consumption are reduced by tag-only packaging and pre-cleanup.
- Affected dependencies/systems:
  - Uses official GitHub Actions for dependency review and CodeQL scanning.
  - Requires repository branch protection/check configuration alignment.
- Test and documentation impact:
  - Workflow validation is required through PR and release dry-runs.
  - Documentation updates are required for CI/release operating procedures and coverage visibility expectations.
- Runtime/API impact:
  - No backend schema changes, no API contract changes, and no end-user feature behavior changes.

## Non-Goals

- No implementation of external coverage dashboards (Codecov/Coveralls) in this change.
- No introduction of strict minimum coverage thresholds as merge blockers in this first rollout.
- No redesign of application functionality, UI behavior, or domain/business logic.
- No deployment environment approval workflow redesign beyond current release process.

## Rollback Plan

- Restore the previous single-workflow CI/CD behavior from git history if workflow split causes instability.
- Revert branch protection required checks to prior configuration if merges are unexpectedly blocked.
- Temporarily disable new security workflows while preserving baseline build/test validation if needed.
- Revert tag-only packaging trigger and pre-clean retention rules if release operations require temporary fallback.
- Re-run baseline build/test and Windows packaging smoke validation after rollback to confirm restored behavior.
