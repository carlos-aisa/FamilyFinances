## 1. Baseline and Collision Inventory

- [x] 1.1 Capture current Windows publish overlap metrics by generating API and Web publishes (`win-x64`, self-contained) and recording file-count/hash statistics in `D:/Programacion/FamilyFinances/openspec/changes/windows-distribution-shared-binaries-layout/design.md` assumptions section (for reproducibility during implementation).
- [x] 1.2 Add a deterministic collision inventory helper script at `D:/Programacion/FamilyFinances/tools/dist/analyze-publish-collisions.ps1` that prints: `API_FILES`, `WEB_FILES`, `COMMON`, `IDENTICAL`, `DIFFERENT`, and the explicit `DIFFERENT` file list.
- [x] 1.3 Validate that the collision inventory script exits non-zero when either publish folder is missing and prints a clear error message (to avoid false green packaging diagnostics).

## 2. Dependency Convergence for Shared Runtime Root

- [x] 2.1 Align package versions so same-name runtime assemblies can coexist as a single root copy by updating `D:/Programacion/FamilyFinances/src/FamilyFinances.Api/FamilyFinances.Api.csproj` (and central package file if introduced) to eliminate `Microsoft.Extensions.*` / `System.*` version drift versus Web publish.
- [x] 2.2 If central package management is introduced, add `D:/Programacion/FamilyFinances/Directory.Packages.props` with explicit pinned versions and update affected `.csproj` files to reference centralized versions only.
- [x] 2.3 Re-run publish collision analysis after dependency convergence and verify that non-config same-name hash-different collisions are reduced to zero or an explicit allowlist-justified set.
- [x] 2.4 Document the final convergence decision and exact package-version rationale in `D:/Programacion/FamilyFinances/openspec/changes/windows-distribution-shared-binaries-layout/design.md` under Decisions.

## 3. Packaged-Mode Configuration Resolution

- [x] 3.1 Update API startup configuration loading in `D:/Programacion/FamilyFinances/src/FamilyFinances.Api/Program.cs` to support packaged-mode config root through an environment variable contract (`FF_CONFIG_ROOT`) while preserving default local/dev behavior when variable is absent.
- [x] 3.2 Update Web startup configuration loading in `D:/Programacion/FamilyFinances/src/FamilyFinances.Web/Program.cs` with the same packaged-mode contract (`FF_CONFIG_ROOT`) and fallback behavior.
- [x] 3.3 Ensure production path values used in packaged mode (`data`, `logs`) remain valid from shared runtime root by updating packaged appsettings sources at build time rather than relying on old `..\\data`/`..\\logs` assumptions.
- [x] 3.4 Add focused tests for packaged config resolution behavior (presence/absence of `FF_CONFIG_ROOT`) in the relevant test projects (`D:/Programacion/FamilyFinances/tests/FamilyFinances.Api.IntegrationTests` and/or `D:/Programacion/FamilyFinances/tests/FamilyFinances.Web.Tests`) so behavior is deterministic and regression-safe.

## 4. Local Windows Packaging Script Refactor

- [x] 4.1 Refactor `D:/Programacion/FamilyFinances/build-windows-dist.ps1` to build final output using one shared runtime root (no duplicated full `api/` + `web/` runtime trees).
- [x] 4.2 Implement hash-aware merge helpers in `build-windows-dist.ps1` using explicit rules: copy-if-missing, skip-if-identical, fail-if-different-unless-config-file.
- [x] 4.3 Implement config redirection logic in `build-windows-dist.ps1` so app-specific files (`appsettings.json`, `appsettings.Production.json`, `web.config`) are copied into `config/api` and `config/web`.
- [x] 4.4 Ensure Web static/runtime content required at runtime (`wwwroot`, localization resources) is present in final root layout without duplicating full publish trees.
- [x] 4.5 Add pre-zip validation in `build-windows-dist.ps1` for required files/folders (both exes, both deps/runtimeconfig files, config folders, `wwwroot`, `data`, `logs`, scripts).
- [x] 4.6 Fail packaging with actionable error output that lists unresolved collisions by relative path and source side.

## 5. Start/Stop Scripts and Distribution Runtime Behavior

- [x] 5.1 Update `D:/Programacion/FamilyFinances/dist/Start FamilyFinances.bat` to launch both executables from shared runtime root and set per-process `FF_CONFIG_ROOT` values (`config\\api` then `config\\web`).
- [x] 5.2 Preserve existing API-first startup sequence and health-check gate in `Start FamilyFinances.bat` (Web must start only after API readiness succeeds).
- [x] 5.3 Update `D:/Programacion/FamilyFinances/dist/Stop FamilyFinances.bat` only as needed for new layout assumptions while keeping process-name-based stop semantics unchanged.
- [x] 5.4 Validate PID file creation/cleanup behavior (`api.pid`, `web.pid`) with the new single-runtime launch model.

## 6. CI Workflow Parity

- [x] 6.1 Update Windows packaging steps in `D:/Programacion/FamilyFinances/.github/workflows/ci.yml` to mirror local merge logic (single runtime root + config redirection + conflict failure).
- [x] 6.2 Update CI required-file verification in `.github/workflows/ci.yml` to assert the new layout (root exes, config directories, static assets, data/log folders).
- [x] 6.3 Ensure CI ZIP artifact naming remains unchanged (`FamilyFinances-v<version>-win-x64.zip`) while containing the new layout.
- [x] 6.4 Keep release upload and cleanup steps functional after layout migration and validate no path assumptions still reference old `api/` + `web/` full trees.

## 7. Documentation Updates

- [x] 7.1 Update `D:/Programacion/FamilyFinances/dist/README.txt` structure diagram and troubleshooting instructions to match the new packaged layout exactly.
- [x] 7.2 Update root repository documentation (`D:/Programacion/FamilyFinances/README.md`) Windows distribution section to describe single shared runtime packaging and operational flow.
- [x] 7.3 Ensure all new/updated documentation text is English and includes explicit rollback notes for maintainers.

## 8. Validation and Release Readiness

- [x] 8.1 Run `dotnet build -nologo` and ensure clean compilation after packaging/config changes.
- [x] 8.2 Run `dotnet test -nologo` and ensure all existing and newly added tests pass.
- [x] 8.3 Generate a local Windows ZIP with `build-windows-dist.ps1`, extract it, and run smoke validation: start, API `/health` ready, Web available at `http://localhost:5019`, stop succeeds.
- [x] 8.4 Confirm no API/OpenAPI contract files, database migrations, or domain behavior changes were introduced by this packaging-focused change.
- [x] 8.5 Record before/after ZIP size comparison (bytes and percentage) in change notes to verify storage optimization objective.
