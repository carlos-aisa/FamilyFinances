## Context

The repository currently uses one mixed workflow (`.github/workflows/ci.yml`) for build/test, reporting gate, Windows packaging, ZIP smoke tests, release publishing, and release ZIP cleanup. This centralization works functionally but has three systemic issues:

1. Quality and security checks are not expressed as dedicated, reusable merge gates.
2. Release packaging runs more often than needed, increasing runner usage and storage pressure.
3. ZIP retention cleanup executes late in the flow, which can fail to protect publish if storage pressure appears earlier.

The proposal for this change introduces:
- Dedicated quality and security workflows for pull requests and branch pushes.
- Windows release packaging only for version tags.
- Pre-publish ZIP cleanup keeping exactly 2 historical ZIP assets (new release creates the third recent ZIP).
- Branch policy where `main` is merge-blocking and `develop` remains informational.

Stakeholders:
- Maintainers shipping tagged Windows releases.
- Contributors opening PRs to `main` and `develop`.
- Repository operators responsible for branch protection and Actions storage health.

Constraints:
- No product runtime behavior changes.
- No API, DB schema, or business logic modifications.
- No external coverage SaaS in this phase.
- Keep existing release smoke/verification quality bar.

## Goals / Non-Goals

**Goals:**
- Create a deterministic CI architecture with clear separation of quality, security, and release responsibilities.
- Provide coverage outputs in GitHub Actions for PR observability.
- Add dependency and code scanning as first-class checks.
- Reduce storage pressure by limiting packaging frequency and cleaning ZIP assets before publish.
- Preserve release reliability through existing structure verification and smoke-test gates.

**Non-Goals:**
- Integrate Codecov/Coveralls in this change.
- Introduce strict coverage percentage thresholds as merge blockers.
- Redesign deployment environments or manual approval gates.
- Change domain/application features.

## IMPLEMENTATION RULES - DO NOT DEVIATE

- [MUST] Create dedicated workflows for quality, dependency, code scanning, release, and Actions artifact cleanup:
  - `ci-quality.yml`
  - `dependency-review.yml`
  - `codeql.yml`
  - `release-windows.yml`
  - `actions-artifacts-cleanup.yml`
- [MUST] Restrict Windows ZIP packaging to `push` events on `v*.*.*` tags only.
- [MUST] Remove/disable legacy paths that package on `main`/`develop` pushes.
- [MUST] Run release ZIP cleanup **before** creating/uploading/publishing the new ZIP.
- [MUST] Keep exactly 2 older ZIP assets (`FamilyFinances-v*-win-x64.zip`) during pre-cleanup.
- [MUST] Preserve existing packaging verification and smoke-test behavior in release flow.
- [MUST] Publish coverage summary in `ci-quality` run summary and keep raw TRX/coverage artifacts PR-scoped and best effort.
- [MUST] Keep checks required for `main` and informational for `develop`.
- [MUST] Use least-privilege job permissions (write only where required).
- [MUST NOT] introduce `release` event publishing path in addition to tag push if it can cause duplicate release uploads.
- [MUST NOT] add external secrets or external SaaS dependencies for coverage.
- [MUST NOT] change API contracts, migrations, or business logic.

## DETAILED UI FLOWS AND WORKFLOW EXECUTION FLOWS

### Flow 1: Pull Request to `main`
1. Contributor opens/updates PR targeting `main`.
2. GitHub triggers:
   - `ci-quality.yml`
   - `dependency-review.yml`
   - `codeql.yml`
3. `ci-quality` restores, builds, runs tests with coverage collector, publishes summary coverage in run UI, and attempts PR-scoped TRX/Cobertura artifacts.
4. `dependency-review` analyzes dependency diffs introduced by the PR.
5. `codeql` analyzes C# code for security/code quality patterns.
6. PR checks panel shows all three checks.
7. Merge remains blocked until all required checks pass.

### Flow 2: Push to `develop` branch (informational quality/security signal)
1. Contributor pushes changes to `develop`.
2. `ci-quality` and `codeql` execute for branch signal.
3. `dependency-review` does not execute because it is PR-scoped to `main`.
4. Results remain informational and do not enforce merge blocking for `develop`.

### Flow 3: Push to `main` or `develop`
1. Maintainer pushes directly to `main` or `develop`.
2. `ci-quality` runs on both branches.
3. `codeql` runs on both branches.
4. `dependency-review` does not run (PR-only).
5. `release-windows` does not run.

### Flow 4: Push Tag `v*.*.*` (Release Path)
1. Maintainer pushes a release tag (example `v0.10.0`).
2. `release-windows.yml` runs.
3. Pre-clean step queries releases and deletes matching ZIP assets beyond newest 2 retained.
4. Workflow builds distribution ZIP, verifies required structure, executes smoke tests.
5. Workflow creates/updates GitHub release and uploads new ZIP.
6. Final expected state: two historical ZIPs retained + new ZIP uploaded.

### Flow 5: Coverage Visibility in PR
1. `ci-quality` finishes test execution.
2. Coverage summary is rendered in GitHub Actions run summary.
3. PR reviewer opens Checks -> `ci-quality`.
4. Reviewer inspects summary/logs and optionally downloads short-lived artifacts for deep inspection.

### Flow 6: Storage Pressure Scenario
1. Tag push starts `release-windows`.
2. Pre-clean executes before build/upload.
3. Storage headroom is reclaimed before new ZIP upload.
4. Publish failure probability from release ZIP accumulation decreases.

## DETAILED PAGE WIREFRAMES

### Pull Request checks (target `main`)

```text
+----------------------------------------------------------------------------------+
| PR #123: github-actions split                                                    |
+----------------------------------------------------------------------------------+
| Checks                                                                           |
|  [x] ci-quality                 Passed    (Required)                             |
|  [x] dependency-review          Passed    (Required)                             |
|  [x] codeql / analyze (csharp)  Passed    (Required)                             |
|                                                                                  |
|  Merge button: ENABLED only if all required checks pass                          |
+----------------------------------------------------------------------------------+
```

### Pull Request checks (target `develop`)

```text
+----------------------------------------------------------------------------------+
| PR #124: docs tweak                                                              |
+----------------------------------------------------------------------------------+
| Checks                                                                           |
|  [x] ci-quality                 Passed    (Informational)                        |
|  [!] dependency-review          Failed    (Informational)                        |
|  [x] codeql / analyze (csharp)  Passed    (Informational)                        |
|                                                                                  |
|  Merge button: still available (policy decided outside required checks)          |
+----------------------------------------------------------------------------------+
```

### Release assets before/after pre-clean

```text
Before tag publish:
  release v0.9.8 -> FamilyFinances-v0.9.8-win-x64.zip
  release v0.9.7 -> FamilyFinances-v0.9.7-win-x64.zip
  release v0.9.6 -> FamilyFinances-v0.9.6-win-x64.zip
  release v0.9.5 -> FamilyFinances-v0.9.5-win-x64.zip

Pre-clean keep=2:
  keep v0.9.8, v0.9.7
  delete v0.9.6, v0.9.5

After new publish (v0.9.9):
  v0.9.9 (new) + v0.9.8 + v0.9.7
```

### Actions workflow list (expected)

```text
.github/workflows/
  ci-quality.yml
  dependency-review.yml
  codeql.yml
  release-windows.yml
  actions-artifacts-cleanup.yml
```

## COMPONENT REUSE MATRIX

| Area | Reuse | Modify | New |
|---|---|---|---|
| Checkout and .NET setup actions | `actions/checkout@v4`, `actions/setup-dotnet@v4` | N/A | No |
| Coverage summary publication | `dotnet-reportgenerator-globaltool` | generate GitHub summary-friendly markdown | No |
| Test artifact publication | `actions/upload-artifact@v4` | PR-scoped, best-effort TRX/coverage upload with short retention | No |
| Release creation | `softprops/action-gh-release@v1` | restrict trigger to tags-only workflow | No |
| ZIP cleanup scripting | `actions/github-script@v7` cleanup logic | move to pre-publish position, keepCount=2 | No |
| Actions artifact cleanup | `actions/github-script@v7` | scheduled/manual deletion of old artifacts | `actions-artifacts-cleanup.yml` |
| Quality validation workflow | existing build/test command patterns | scope and triggers redefined | `ci-quality.yml` |
| Dependency risk workflow | N/A | N/A | `dependency-review.yml` |
| Code scanning workflow | N/A | N/A | `codeql.yml` |
| Windows release workflow | existing packaging steps from current `ci.yml` | split from mixed workflow, retargeted trigger | `release-windows.yml` |

## Decisions

### Decision 1: Split mixed CI/CD into dedicated workflows with operational cleanup
- **Choice:** Separate quality, dependency review, code scanning, release packaging, and Actions artifact cleanup flows.
- **Rationale:** Clear ownership, faster troubleshooting, and explicit check identities for branch protection.
- **Alternative considered:** Keep one monolithic workflow.
  - **Rejected because:** hard to maintain, difficult to enforce precise required checks, and couples unrelated failures.

### Decision 2: Keep merge-blocking checks only on `main`
- **Choice:** Required checks for `main`; `develop` gets signal-only checks.
- **Rationale:** Maintains release quality gate without over-constraining daily integration branch.
- **Alternative considered:** Required checks for both `main` and `develop`.
  - **Rejected because:** increases integration friction and may block fast iterations.

### Decision 3: Package Windows release only on version tags
- **Choice:** Trigger `release-windows.yml` only on `push` tags matching `v*.*.*`.
- **Rationale:** Major reduction in unnecessary ZIP builds and artifact churn.
- **Alternative considered:** Continue packaging on `main`/`develop`.
  - **Rejected because:** direct contributor-reported storage pressure and operational cost.

### Decision 4: Cleanup ZIP assets before publish and retain 2 historical ZIPs
- **Choice:** Pre-cleanup step before packaging/publish, keep exactly 2 older ZIP assets.
- **Rationale:** Ensures storage headroom exists before upload; aligns with requested retention model.
- **Alternative considered:** Cleanup only after publish.
  - **Rejected because:** cannot prevent pre-upload storage failures.

### Decision 5: First-phase coverage visibility via native GitHub run summary plus PR artifacts
- **Choice:** Publish coverage summary directly in `ci-quality` run summary and keep raw TRX/Cobertura artifacts PR-scoped and best effort; no external service in this change.
- **Rationale:** Zero external dependency, immediate reviewer visibility, and lower storage pressure.
- **Alternative considered:** Integrate Codecov immediately.
  - **Rejected because:** out-of-scope and requires additional policy/secret setup.

### Decision 6: Use CodeQL with PR + push + scheduled execution
- **Choice:** Run CodeQL on PR, on branch pushes, and weekly schedule.
- **Rationale:** Covers both code review time and drift between PRs.
- **Alternative considered:** PR-only scanning.
  - **Rejected because:** misses newly introduced risks from direct pushes and dependency ecosystem drift over time.

### Decision 7: Run Dependency Review on PR events only
- **Choice:** Execute dependency review only for pull requests.
- **Rationale:** Dependency diff relevance is PR-centric.
- **Alternative considered:** Run on all pushes.
  - **Rejected because:** no dependency delta context for direct push-only checks and unnecessary compute.

### Decision 8: Preserve existing release verification and smoke script gates
- **Choice:** Reuse current required-content verification and `smoke-windows-dist.ps1`.
- **Rationale:** Keep release confidence baseline and avoid regression in packaging quality.
- **Alternative considered:** Simplify release by removing smoke tests.
  - **Rejected because:** raises regression risk in distributable startup behavior.

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: `ci-quality.yml` skeleton

```yaml
name: CI Quality

on:
  pull_request:
    branches: [ "main" ]
  push:
    branches: [ "main", "develop" ]

jobs:
  ci-quality:
    runs-on: ubuntu-latest
    permissions:
      contents: read
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "9.0.x"
      - run: dotnet restore
      - run: dotnet build --configuration Release --no-restore
      - run: dotnet test --configuration Release --no-build --collect:"XPlat Code Coverage" --logger "trx;LogFileName=test-results.trx"
      - name: Generate coverage summary
        run: |
          dotnet tool install --global dotnet-reportgenerator-globaltool
          export PATH="$PATH:$HOME/.dotnet/tools"
          reportgenerator -reports:"**/TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:"MarkdownSummaryGithub;TextSummary"
          cat coveragereport/SummaryGithub.md >> "$GITHUB_STEP_SUMMARY"
        shell: bash
      - name: Upload test and coverage artifacts (PR only, best effort)
        if: github.event_name == 'pull_request'
        continue-on-error: true
        uses: actions/upload-artifact@v4
        with:
          name: ci-quality-results-${{ github.run_id }}
          path: |
            **/TestResults/**/*.trx
            **/TestResults/**/coverage.cobertura.xml
          retention-days: 2
```

### Example 2: `dependency-review.yml` skeleton

```yaml
name: Dependency Review

on:
  pull_request:
    branches: [ "main" ]

jobs:
  dependency-review:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      pull-requests: read
    steps:
      - name: Dependency Review
        uses: actions/dependency-review-action@v4
```

### Example 3: `codeql.yml` skeleton

```yaml
name: CodeQL

on:
  pull_request:
    branches: [ "main" ]
  push:
    branches: [ "main", "develop" ]
  schedule:
    - cron: "0 5 * * 1"

jobs:
  analyze:
    runs-on: ubuntu-latest
    permissions:
      actions: read
      contents: read
      security-events: write
    steps:
      - uses: actions/checkout@v4
      - uses: github/codeql-action/init@v4
        with:
          languages: csharp
      - uses: github/codeql-action/autobuild@v4
      - uses: github/codeql-action/analyze@v4
```

### Example 4: `release-windows.yml` trigger and pre-clean sequencing

```yaml
name: Release Windows Distribution

on:
  push:
    tags:
      - "v*.*.*"

jobs:
  release-windows:
    runs-on: windows-latest
    permissions:
      contents: write
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "9.0.x"

      - name: Pre-clean old release ZIP assets (keep latest 2)
        uses: actions/github-script@v7
        with:
          github-token: ${{ secrets.GITHUB_TOKEN }}
          script: |
            const keepCount = 2;
            // Delete matching ZIP assets from older releases before packaging/publish.

      - name: Build distribution
        run: ./build-windows-dist.ps1 -Version "${{ github.ref_name#v }}" -Configuration Release
        shell: pwsh
```

### Example 5: GitHub Script pre-clean logic (copy-ready core)

```javascript
const owner = context.repo.owner;
const repo = context.repo.repo;
const keepCount = 2;
const zipAssetPattern = /^FamilyFinances-v.*-win-x64\.zip$/i;

const releases = await github.paginate(github.rest.repos.listReleases, {
  owner,
  repo,
  per_page: 100
});

const orderedReleases = releases
  .filter(r => !r.draft)
  .sort((a, b) => {
    const aDate = new Date(a.published_at || a.created_at).getTime();
    const bDate = new Date(b.published_at || b.created_at).getTime();
    return bDate - aDate;
  });

for (const release of orderedReleases.slice(keepCount)) {
  for (const asset of release.assets || []) {
    if (!zipAssetPattern.test(asset.name)) continue;
    await github.rest.repos.deleteReleaseAsset({ owner, repo, asset_id: asset.id });
  }
}
```

## Risks / Trade-offs

- [Risk] New workflows increase total check count and perceived CI complexity.
  - Mitigation: Use clear workflow names and stable job ids; document each purpose.

- [Risk] Branch protection can block merges if required check names mismatch job names.
  - Mitigation: Freeze job names before enabling required checks; validate with test PR.

- [Risk] Pre-clean script may accidentally delete non-target assets if filter is broad.
  - Mitigation: Use strict ZIP filename regex and test on dry-run branch first.

- [Risk] CodeQL runtime can lengthen CI time.
  - Mitigation: Keep CodeQL in dedicated workflow and avoid coupling with release flow.

- [Risk] Coverage artifacts provide data but not rich trend UI.
  - Mitigation: Defer Codecov/Coveralls as explicit follow-up change.

- [Risk] Legacy `ci.yml` overlap may duplicate execution until cleanup is complete.
  - Mitigation: Remove or narrow legacy triggers as part of migration steps.

- [Trade-off] `develop` not merge-blocked means potential quality regressions can still pass.
  - Mitigation: Maintain signal checks and reinforce review policy for develop merges.

## Migration Plan

1. Create `ci-quality.yml` and validate PR/push execution.
2. Create `dependency-review.yml` and validate PR dependency diff scanning.
3. Create `codeql.yml` and validate initial analysis upload.
4. Create `release-windows.yml` with tag-only trigger, pre-clean keep=2, and existing packaging verification/smoke steps.
5. Remove or reduce overlapping behavior in legacy `ci.yml` to avoid duplicate runs.
6. Run a test PR to `main` and verify required-check suitability.
7. Run a test PR to `develop` and verify informational behavior.
8. Configure branch protection:
   - `main`: require `ci-quality`, `dependency-review`, and `codeql` checks.
   - `develop`: do not require those checks.
9. Validate tagged release dry run in a non-production tag series if needed.
10. Confirm release asset retention behavior:
    - pre-clean keeps 2 old ZIPs,
    - new publish results in 3 recent ZIPs total.

### Rollback Strategy

1. Re-enable old `ci.yml` paths (or revert workflow split commit) if release/CI disruption occurs.
2. Remove required checks from `main` branch protection temporarily if merge pipeline is blocked unexpectedly.
3. Disable newly added security workflows while retaining baseline build/test if urgent unblocking is required.
4. Restore prior post-publish cleanup behavior only as temporary fallback.
5. Re-run baseline tests and packaging smoke scripts after rollback.

## Open Questions

- Should release workflow also support manual `workflow_dispatch` for emergency republish scenarios, or remain tag-only strict in this phase?
- Should we pin third-party actions to commit SHAs now as part of this change, or defer to a separate hardening change?

## IMPLEMENTATION VERIFICATION CHECKLIST

### Workflow Architecture and Trigger Scope
- [ ] Confirm `.github/workflows/ci-quality.yml` exists and is valid YAML.
- [ ] Confirm `.github/workflows/dependency-review.yml` exists and is valid YAML.
- [ ] Confirm `.github/workflows/codeql.yml` exists and is valid YAML.
- [ ] Confirm `.github/workflows/release-windows.yml` exists and is valid YAML.
- [ ] Confirm Windows packaging no longer runs on `push` to `main`.
- [ ] Confirm Windows packaging no longer runs on `push` to `develop`.
- [ ] Confirm release workflow triggers on `push` tags matching `v*.*.*`.
- [ ] Confirm legacy `ci.yml` no longer duplicates release packaging path.
- [ ] Confirm workflow names/jobs are stable for branch protection configuration.
- [ ] Confirm no workflow introduces unrelated product behavior changes.

### CI Quality Workflow
- [ ] Confirm `ci-quality` runs on PRs to `main`.
- [ ] Confirm `ci-quality` runs on pushes to `main`.
- [ ] Confirm `ci-quality` runs on pushes to `develop`.
- [ ] Confirm `dotnet restore` runs successfully in quality workflow.
- [ ] Confirm `dotnet build --configuration Release --no-restore` runs successfully.
- [ ] Confirm `dotnet test --configuration Release --no-build` runs successfully.
- [ ] Confirm XPlat coverage collection is enabled in test step.
- [ ] Confirm TRX test result files are generated.
- [ ] Confirm Cobertura coverage files are generated.
- [ ] Confirm coverage summary appears in run summary.
- [ ] Confirm PR-scoped TRX and coverage files are uploaded as best-effort artifacts.
- [ ] Confirm PR artifact retention is explicitly configured to low values.
- [ ] Confirm workflow summary includes enough info for reviewers.

### Dependency Review Workflow
- [ ] Confirm dependency-review runs only on pull requests.
- [ ] Confirm dependency-review targets `main`.
- [ ] Confirm workflow permissions are read-scoped (`contents`, `pull-requests`).
- [ ] Confirm dependency-review action version is explicit.
- [ ] Confirm dependency-review check appears in PR checks list.
- [ ] Confirm dependency risk findings surface clearly in check output.
- [ ] Confirm no false trigger on non-PR events.
- [ ] Confirm workflow does not require secrets beyond `GITHUB_TOKEN`.

### CodeQL Workflow
- [ ] Confirm CodeQL runs on PRs to `main`.
- [ ] Confirm CodeQL runs on pushes to `main`.
- [ ] Confirm CodeQL runs on pushes to `develop`.
- [ ] Confirm CodeQL schedule trigger is configured weekly.
- [ ] Confirm CodeQL language is set to `csharp`.
- [ ] Confirm `security-events: write` permission is present for analyze job.
- [ ] Confirm analyze results appear in repository Security / Code scanning UI.
- [ ] Confirm no release packaging dependency on CodeQL workflow.

### Release Workflow and Storage Control
- [ ] Confirm release workflow starts only on `v*.*.*` tag push.
- [ ] Confirm release workflow has `contents: write` permission.
- [ ] Confirm pre-clean ZIP cleanup step executes before packaging.
- [ ] Confirm cleanup regex matches only `FamilyFinances-v*-win-x64.zip`.
- [ ] Confirm cleanup retains exactly 2 older ZIP assets.
- [ ] Confirm cleanup does not delete non-ZIP release assets.
- [ ] Confirm Windows distribution build script still runs successfully.
- [ ] Confirm required distribution file/dir verification still runs.
- [ ] Confirm smoke test script still runs against produced ZIP.
- [ ] Confirm release publish uploads ZIP successfully after pre-clean.
- [ ] Confirm post-release recent ZIP set equals new + 2 previous.
- [ ] Confirm release workflow does not upload duplicate Actions ZIP artifacts.

### Branch Protection and Policy Alignment
- [ ] Confirm `main` branch protection requires `ci-quality`.
- [ ] Confirm `main` branch protection requires `dependency-review`.
- [ ] Confirm `main` branch protection requires `analyze`.
- [ ] Confirm PR to `main` cannot merge while required checks fail.
- [ ] Confirm `develop` branch protection does not require those checks.
- [ ] Confirm pushes to `develop` still run checks as informational signal.
- [ ] Confirm maintainers understand required vs informational policy.

### Documentation and Operational Readiness
- [ ] Confirm CI/release docs are updated with new workflow architecture.
- [ ] Confirm docs mention coverage visibility path in GitHub checks summary/PR artifacts.
- [ ] Confirm docs mention external coverage UI (Codecov/Coveralls) is deferred.
- [ ] Confirm docs mention release tag-only packaging policy.
- [ ] Confirm docs mention pre-clean keep=2 ZIP retention behavior.
- [ ] Confirm rollback instructions are documented and tested at least once.
