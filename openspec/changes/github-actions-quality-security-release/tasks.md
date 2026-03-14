## 1. Workflow split baseline and file scaffolding

- [x] 1.1 Capture the current responsibilities of `d:/Programacion/FamilyFinances/.github/workflows/ci.yml` (quality checks, reporting gate, release packaging, release publish, ZIP cleanup) into a migration checklist comment block in the change notes or PR description.
- [x] 1.2 Create `d:/Programacion/FamilyFinances/.github/workflows/ci-quality.yml` with stable workflow/job names (`CI Quality` / `ci-quality`) so branch protection check names can be pinned.
- [x] 1.3 Create `d:/Programacion/FamilyFinances/.github/workflows/dependency-review.yml` with stable workflow/job names (`Dependency Review` / `dependency-review`).
- [x] 1.4 Create `d:/Programacion/FamilyFinances/.github/workflows/codeql.yml` with stable workflow/job names (`CodeQL` / `analyze`).
- [x] 1.5 Create `d:/Programacion/FamilyFinances/.github/workflows/release-windows.yml` with stable workflow/job name (`Release Windows Distribution` / `release-windows`).

## 2. Implement quality workflow with test and coverage evidence

- [x] 2.1 In `d:/Programacion/FamilyFinances/.github/workflows/ci-quality.yml`, configure triggers for:
  - `pull_request` on `main`
  - `push` on `main` and `develop`
- [x] 2.2 Add build pipeline steps in `ci-quality.yml`: `actions/checkout@v4`, `actions/setup-dotnet@v4` (`9.0.x`), `dotnet restore`, `dotnet build --configuration Release --no-restore`.
- [x] 2.3 Add test step in `ci-quality.yml` using `dotnet test --configuration Release --no-build --collect:"XPlat Code Coverage" --logger "trx;LogFileName=test-results.trx"`.
- [x] 2.4 Add PR-scoped, best-effort artifact upload in `ci-quality.yml` with `actions/upload-artifact@v4` for:
  - `**/TestResults/**/*.trx`
  - `**/TestResults/**/coverage.cobertura.xml`
- [x] 2.5 Add explicit `permissions` in `ci-quality.yml` with read-only scope (`contents: read`) and verify no write permission is granted.
- [x] 2.6 Add run summary output in `ci-quality.yml` that includes inline coverage summary and artifact guidance.

## 3. Implement security scanning workflows

- [x] 3.1 In `d:/Programacion/FamilyFinances/.github/workflows/dependency-review.yml`, configure PR-only trigger for `main`.
- [x] 3.2 In `dependency-review.yml`, add `actions/dependency-review-action@v4` and restrict permissions to read scopes needed for PR dependency analysis.
- [x] 3.3 In `d:/Programacion/FamilyFinances/.github/workflows/codeql.yml`, configure triggers for:
  - `pull_request` on `main`
  - `push` on `main` and `develop`
  - weekly `schedule` cron
- [x] 3.4 In `codeql.yml`, configure `github/codeql-action/init@v4` with `languages: csharp`, and include autobuild + analyze steps.
- [x] 3.5 In `codeql.yml`, set permissions so `security-events: write` exists for result publishing and all other permissions are minimal.
- [x] 3.6 Add private-repo guard (`if: !github.event.repository.private`) to `codeql.yml` and `dependency-review.yml` so workflows skip cleanly when Code Security/GHAS is unavailable.

## 4. Implement tag-only Windows release workflow with pre-clean keep=2

- [x] 4.1 In `d:/Programacion/FamilyFinances/.github/workflows/release-windows.yml`, configure trigger only for `push` tags matching `v*.*.*`.
- [x] 4.2 Port release packaging steps from legacy CI into `release-windows.yml`: version extraction, `build-windows-dist.ps1`, distribution structure verification, ZIP smoke test (`d:/Programacion/FamilyFinances/tools/dist/smoke-windows-dist.ps1`), and GitHub release publish.
- [x] 4.3 Add pre-publish cleanup step in `release-windows.yml` (before build/publish) using `actions/github-script@v7` to delete matching assets with regex `^FamilyFinances-v.*-win-x64\\.zip$` beyond the latest 2 retained.
- [x] 4.4 Ensure cleanup logic in `release-windows.yml` ignores draft releases and does not delete non-matching assets.
- [x] 4.5 Avoid duplicate Actions artifact ZIP upload in `release-windows.yml` so release ZIP storage remains only in GitHub Releases.
- [x] 4.6 Ensure `release-windows.yml` uses `permissions: contents: write` only where release asset management requires it.
- [x] 4.7 Add scheduled/manual Actions artifact cleanup workflow to reduce storage pressure over time.

## 5. Remove overlap from legacy mixed workflow

- [x] 5.1 Refactor or remove packaging/release jobs from `d:/Programacion/FamilyFinances/.github/workflows/ci.yml` so Windows packaging is not executed from branch pushes anymore.
- [x] 5.2 Ensure no duplicate release upload path remains (avoid running publish from both `push tag` and `release` event flows simultaneously).
- [x] 5.3 Keep or move manual ZIP cleanup capability (`workflow_dispatch`) into a dedicated workflow file only if still needed operationally; if kept, set default `keep_count` to `2`.
- [ ] 5.4 Verify expected check identities in PR UI are now:
  - `ci-quality`
  - `dependency-review`
  - `analyze` (CodeQL job)

## 6. Validation and behavioral test updates

- [x] 6.1 Validate workflow YAML syntax and structure for all new workflow files under `d:/Programacion/FamilyFinances/.github/workflows/`.
- [x] 6.2 Run repository tests locally after workflow refactor with `dotnet test d:/Programacion/FamilyFinances/FamilyFinances.sln -c Release` and resolve regressions.
- [ ] 6.3 Open a PR targeting `main` and verify the three required checks execute and appear in GitHub checks UI with expected names.
- [ ] 6.4 Push to `develop` and verify quality/security checks run as informational signal for that branch.
- [ ] 6.5 Push a disposable semver tag (for example `v0.0.0`) in a controlled validation branch/repo context and verify `release-windows.yml` is the only packaging workflow that runs.
- [ ] 6.6 In release run logs, verify pre-clean step executes before ZIP publish and keeps only 2 historical matching ZIP assets.
- [ ] 6.7 Verify post-publish state contains exactly 3 recent matching ZIP assets (new + two previous).

## 7. Documentation and branch protection rollout

- [x] 7.1 Update `d:/Programacion/FamilyFinances/README.md` with the new workflow architecture (quality, dependency review, codeql, release-windows) and branch policy (`main` required, `develop` informational).
- [x] 7.2 Update `d:/Programacion/FamilyFinances/docs/windows-distribution-build.md` to document tag-only release packaging and pre-clean `keep=2` retention behavior.
- [x] 7.3 Add or update CI governance documentation (for example `d:/Programacion/FamilyFinances/docs/github-actions-governance.md`) with:
  - required checks for `main`
  - informational policy for `develop`
  - where to find coverage outputs in GitHub checks summary and PR artifacts
- [ ] 7.4 Configure repository branch protection in GitHub settings:
  - `main`: require `ci-quality`, `dependency-review`, and `analyze`
  - `develop`: do not require these checks
- [x] 7.5 Document future enhancement note (outside this change) for "coverage UI bonita" using Codecov/Coveralls and required setup implications.


