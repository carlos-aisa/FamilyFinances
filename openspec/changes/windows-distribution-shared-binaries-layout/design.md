## Context

The current Windows ZIP distribution packages API and Web into separate full publish folders (`api/` and `web/`).
This duplicates most runtime files and increases artifact/release storage usage.

Observed baseline (local publish comparison, win-x64, self-contained, Release):

- API files: 382
- Web files: 519
- Same relative paths across both outputs: 354
- Identical overlaps: 334
- Same name but different content: 20
- Potential dedup saving: ~106.65 MB (~45.22% of combined publish size)
Implementation baseline snapshot (captured during apply):

- Command: `powershell -ExecutionPolicy Bypass -File tools/dist/analyze-publish-collisions.ps1 -ApiPublishDir publish_compare_api -WebPublishDir publish_compare_web`
- Result:
  - `API_FILES=382`
  - `WEB_FILES=519`
  - `COMMON=354`
  - `IDENTICAL=334`
  - `DIFFERENT=20`
  - Non-config differences included `Microsoft.Extensions.*` and `System.*` assemblies.

Current packaging and runtime behavior:

- Local builder creates `dist/<version>/api` and `dist/<version>/web` separately.
- Start script launches `FamilyFinances.Api.exe` from `api/` and `FamilyFinances.Web.exe` from `web/`.
- API and Web own separate `appsettings*.json`, `web.config`, and process contexts.

Stakeholders:

- End users downloading Windows ZIP releases.
- Maintainers operating `build-windows-dist.ps1` and GitHub Actions.
- CI/release owners responsible for artifact storage.

## IMPLEMENTATION RULES - DO NOT DEVIATE

- Keep distribution format as a single ZIP file.
- Do not change API routes, DTO contracts, auth policies, or business behavior.
- Do not change database schema or migration history.
- Do not introduce custom assembly loading mechanisms.
- Do not allow blind overwrite of same-name files with different hashes.
- Do not remove `Start FamilyFinances.bat` and `Stop FamilyFinances.bat` from ZIP root.
- Do not introduce separate full runtime trees (`api/` and `web/`) in final package.
- Ensure both executables start from the shared binary root in the packaged layout.
- Ensure app-specific configuration remains isolated and deterministic.
- Ensure `data/` and `logs/` remain sibling folders under distribution root.
- All packaging/runtime docs and comments must remain in English.

## Goals / Non-Goals

**Goals:**

- Package API and Web binaries into one shared runtime folder with one physical copy of identical files.
- Preserve safe coexistence of API and Web when filenames collide with different content.
- Keep user-facing operation unchanged: start via `Start FamilyFinances.bat`, stop via `Stop FamilyFinances.bat`, open web at `http://localhost:5019`.
- Add deterministic verification to packaging (conflict detection + required-file validation).
- Keep CI and local packaging outputs structurally identical.

**Non-Goals:**

- No feature/UI/business-rule changes in API or Web.
- No new deployment target formats (MSI, Docker, installers).
- No cross-platform packaging redesign.
- No backend localization or auth flow redesign.
- No change to release automation semantics beyond what is needed for the new layout.

## Decisions

### Decision 1: Use a single shared runtime root with app-specific config folders

**Decision:**
Package both executables and shared runtime files into one root runtime context, with app-specific configuration in dedicated subfolders.

Target layout:

```text
FamilyFinances-v<version>-win-x64/
  Start FamilyFinances.bat
  Stop FamilyFinances.bat
  README.txt

  FamilyFinances.Api.exe
  FamilyFinances.Web.exe
  FamilyFinances.Api.dll
  FamilyFinances.Web.dll
  *.deps.json
  *.runtimeconfig.json
  *.dll (shared runtime assemblies, single copy)

  config/
    api/
      appsettings.json
      appsettings.Production.json
      web.config
    web/
      appsettings.json
      appsettings.Production.json
      web.config

  wwwroot/
  en-US/
  es-ES/

  data/
  logs/
```

**Rationale:**
- Eliminates full runtime duplication while preserving per-app config isolation.
- Keeps operator usage simple (same ZIP root scripts).
- Avoids maintaining two complete runtime trees.

**Alternatives considered:**
- Keep `api/` + `web/` full folders: rejected (high duplication remains).
- Put everything in root with shared `appsettings*.json`: rejected (config collisions and ambiguous ownership).
- Single-file publish for both apps: rejected for now (scope/risk increase and diagnostic trade-offs).

### Decision 2: Introduce deterministic merge with hash-aware conflict policy

**Decision:**
Publish API and Web to temporary folders, then merge into final dist root using path+hash policy:

- If relative path does not exist in staging: copy.
- If relative path exists and hash is identical: skip copy (dedup).
- If relative path exists and hash differs:
  - If file is app-specific config (`appsettings*.json`, `web.config`): redirect to `config/api` or `config/web`.
  - Otherwise: fail packaging with explicit error list.

**Rationale:**
- Prevents accidental runtime corruption.
- Makes conflicts explicit and actionable.
- Keeps merge deterministic across local and CI.

**Alternatives considered:**
- Last-write-wins overwrite: rejected (unsafe and non-deterministic runtime behavior).
- Filename suffix renaming for arbitrary collisions: rejected (breaks runtime resolution semantics).

### Decision 3: Eliminate shared-library version drift between API and Web outputs

**Decision:**
Align dependency graph so shared library filenames that must coexist in root are byte-compatible across publishes.
This includes removing known `Microsoft.Extensions.*`/`System.*` version drift introduced transitively.

**Rationale:**
- Current outputs include same-name different-version assemblies (e.g., 10.0.0.0 vs 9.0.0.0), which cannot safely coexist as single file.
- A shared folder requires convergence for same-name runtime assets.

**Alternatives considered:**
- Keep divergent versions and force custom load contexts: rejected (complex, high risk, out of scope).
- Keep duplicated conflicting assemblies under app subfolders: rejected (reintroduces duplication and path complexity).
Implementation confirmation:

- API package update applied: `Serilog.AspNetCore` changed from `10.0.0` to `9.0.0` in `src/FamilyFinances.Api/FamilyFinances.Api.csproj`.
- Re-run command: `powershell -ExecutionPolicy Bypass -File tools/dist/analyze-publish-collisions.ps1 -ApiPublishDir publish_compare_api_v2 -WebPublishDir publish_compare_web_v2`.
- Re-run result:
  - `API_FILES=382`
  - `WEB_FILES=519`
  - `COMMON=354`
  - `IDENTICAL=351`
  - `DIFFERENT=3`
  - Remaining differences: `appsettings.json`, `appsettings.Production.json`, `web.config` (expected app-specific config files only).

### Decision 4: Load app-specific configuration from explicit subpaths in packaged mode

**Decision:**
API and Web startup shall support loading configuration from app-specific config directory (`config/api` or `config/web`) when an environment variable indicates packaged mode.

**Rationale:**
- Avoids appsettings filename collision in root.
- Preserves local development behavior when variable is absent.
- Keeps start scripts in control of packaged runtime context.

**Alternatives considered:**
- Rename appsettings files without startup changes: rejected (ASP.NET default configuration pipeline will not pick renamed files automatically).

### Decision 5: Keep script UX unchanged but update execution model

**Decision:**
Start script launches both executables from distribution root (shared binary location), setting app-specific config context per process.
Stop script continues terminating by executable name.

**Rationale:**
- Preserves operator workflow.
- Supports new single-folder runtime layout.

**Alternatives considered:**
- Replace scripts with PowerShell-only launcher: rejected (changes user expectation and portability of current `.bat` workflow).

## DETAILED OPERATION FLOWS AND SCRIPT REUSE

### Flow 1: Local packaging generation

1. Developer runs `build-windows-dist.ps1` with version/configuration.
2. Script publishes API to temp path.
3. Script publishes Web to temp path.
4. Script initializes final dist structure (`data/`, `logs/`, `config/api`, `config/web`).
5. Script performs merge with hash policy into root.
6. Script routes app-specific config files to dedicated config folders.
7. Script runs required-file and conflict verification.
8. Script creates ZIP.

### Flow 2: Start packaged app

1. User double-clicks `Start FamilyFinances.bat`.
2. Script ensures `data/` and `logs/` exist.
3. Script starts API from root executable path with API config context.
4. Script waits for API health endpoint.
5. Script starts Web from root executable path with Web config context.
6. Script opens browser at `http://localhost:5019`.

### Flow 3: Stop packaged app

1. User double-clicks `Stop FamilyFinances.bat`.
2. Script kills `FamilyFinances.Web.exe` if running.
3. Script kills `FamilyFinances.Api.exe` if running.
4. Script removes PID markers.

### Flow 4: CI packaging

1. GitHub Actions publishes API + Web.
2. CI merge logic creates same structure as local builder.
3. CI verifies file invariants.
4. CI emits ZIP artifact/release asset.

### Flow 5: Conflict detection failure path

1. Merge finds same relative path with different hash and non-config file.
2. Build stops before ZIP generation.
3. Error report lists paths and source publish side.
4. Maintainer aligns dependencies or updates conflict rules explicitly.

## DETAILED LAYOUT WIREFRAMES

### Current (problematic)

```text
ZIP root
+- Start FamilyFinances.bat
+- Stop FamilyFinances.bat
+- api/  (full publish)
+- web/  (full publish)
   -> Large duplication
```

### Target (single shared runtime)

```text
ZIP root
+- Start FamilyFinances.bat
+- Stop FamilyFinances.bat
+- FamilyFinances.Api.exe
+- FamilyFinances.Web.exe
+- *.dll / *.deps.json / *.runtimeconfig.json (shared once)
+- config/
¦  +- api/appsettings*.json + web.config
¦  +- web/appsettings*.json + web.config
+- wwwroot/
+- en-US/
+- es-ES/
+- data/
+- logs/
```

### Start sequence

```text
Start.bat
  |
  +--> start API (root exe + config/api)
  |      |
  |      +--> health check /health
  |
  +--> start Web (root exe + config/web)
         |
         +--> browser http://localhost:5019
```

## COMPONENT REUSE MATRIX

| Area | Existing file/component | Action | Notes |
|---|---|---|---|
| Local builder | `D:/Programacion/FamilyFinances/build-windows-dist.ps1` | Modify | Replace dual-copy folder strategy with merge+dedup strategy |
| Start script | `D:/Programacion/FamilyFinances/dist/Start FamilyFinances.bat` | Modify | Launch from root and set app-specific config context |
| Stop script | `D:/Programacion/FamilyFinances/dist/Stop FamilyFinances.bat` | Modify | Keep process-based stop; update assumptions if needed |
| Distribution docs | `D:/Programacion/FamilyFinances/dist/README.txt` | Modify | Document new layout and troubleshooting paths |
| CI packaging | `D:/Programacion/FamilyFinances/.github/workflows/ci.yml` | Modify | Mirror local merge strategy and validations |
| API startup config | `D:/Programacion/FamilyFinances/src/FamilyFinances.Api/Program.cs` | Modify (minimal) | Add optional packaged-mode config root loading |
| Web startup config | `D:/Programacion/FamilyFinances/src/FamilyFinances.Web/Program.cs` | Modify (minimal) | Add optional packaged-mode config root loading |
| Dependency alignment | `D:/Programacion/FamilyFinances/src/FamilyFinances.Api/FamilyFinances.Api.csproj` (+ optional central props) | Modify | Remove transitive version drift that causes same-name collisions |

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: Hash-aware merge rule (PowerShell)

```powershell
function Merge-PublishTree {
    param(
        [string]$SourceRoot,
        [string]$TargetRoot,
        [string]$SourceTag
    )

    Get-ChildItem $SourceRoot -Recurse -File | ForEach-Object {
        $rel = $_.FullName.Substring((Resolve-Path $SourceRoot).Path.Length + 1)
        $target = Join-Path $TargetRoot $rel

        if (-not (Test-Path $target)) {
            New-Item -ItemType Directory -Path (Split-Path $target) -Force | Out-Null
            Copy-Item $_.FullName $target -Force
            return
        }

        $srcHash = (Get-FileHash $_.FullName -Algorithm SHA256).Hash
        $dstHash = (Get-FileHash $target -Algorithm SHA256).Hash

        if ($srcHash -eq $dstHash) {
            return
        }

        throw "Conflict: $rel differs between sources (existing vs $SourceTag)."
    }
}
```

### Example 2: Redirect config collisions

```powershell
function Copy-AppConfigFiles {
    param(
        [string]$SourceRoot,
        [string]$DistRoot,
        [ValidateSet('api','web')] [string]$App
    )

    $dest = Join-Path $DistRoot "config/$App"
    New-Item -ItemType Directory -Path $dest -Force | Out-Null

    @('appsettings.json', 'appsettings.Production.json', 'web.config') | ForEach-Object {
        $src = Join-Path $SourceRoot $_
        if (Test-Path $src) {
            Copy-Item $src (Join-Path $dest $_) -Force
        }
    }
}
```

### Example 3: Optional packaged-mode config root in Program.cs

```csharp
var packagedConfigRoot = Environment.GetEnvironmentVariable("FF_CONFIG_ROOT");
if (!string.IsNullOrWhiteSpace(packagedConfigRoot))
{
    builder.Configuration.Sources.Clear();
    builder.Configuration
        .AddJsonFile(Path.Combine(packagedConfigRoot, "appsettings.json"), optional: false, reloadOnChange: false)
        .AddJsonFile(Path.Combine(packagedConfigRoot, $"appsettings.{builder.Environment.EnvironmentName}.json"), optional: true, reloadOnChange: false)
        .AddEnvironmentVariables();
}
```

### Example 4: Start script process launch from shared root

```bat
set FF_CONFIG_ROOT=%ROOT_DIR%config\api
start /D "%ROOT_DIR%" /min "FamilyFinances API" "%ROOT_DIR%FamilyFinances.Api.exe"

set FF_CONFIG_ROOT=%ROOT_DIR%config\web
start /D "%ROOT_DIR%" /min "FamilyFinances Web" "%ROOT_DIR%FamilyFinances.Web.exe"
```

### Example 5: CI validation for forbidden unresolved collisions

```powershell
if ($ConflictList.Count -gt 0) {
    Write-Host "ERROR: unresolved merge conflicts detected:" -ForegroundColor Red
    $ConflictList | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    exit 1
}
```

## CRITICAL OPERATOR BEHAVIORS

- Operator still uses the same two `.bat` files; no new operational entry point is introduced.
- API must still be reachable at `http://localhost:5084` and Web at `http://localhost:5019`.
- Browser auto-open behavior remains unchanged.
- If packaging detects unresolved collisions, build must fail early with explicit messages.
- If API health check fails, Web startup must not continue silently.
- PID cleanup behavior remains deterministic on stop.

## Risks / Trade-offs

- [Risk] Dependency alignment may require package version changes with subtle logging/runtime side effects.
  -> Mitigation: pin versions explicitly, run full `dotnet build` + `dotnet test`, and run packaged smoke tests.

- [Risk] Custom configuration loading path could diverge from local dev defaults.
  -> Mitigation: enable packaged-mode only when environment variable is set; preserve default behavior otherwise.

- [Risk] Root-folder runtime may expose new path assumptions in logs/data references.
  -> Mitigation: update production config values and validate data/log creation in packaged smoke tests.

- [Risk] CI and local builder drift over time.
  -> Mitigation: keep equivalent merge/verification logic and verify with the same required-file contract.

- [Trade-off] Additional packaging logic complexity in scripts.
  -> Mitigation: add deterministic helper functions and explicit verification output.

## Migration Plan

1. Add/confirm dependency alignment strategy to remove same-name version drift.
2. Implement hash-aware merge functions in local packaging script.
3. Introduce config redirection (`config/api`, `config/web`) and copy rules.
4. Update API/Web startup configuration loading for packaged mode.
5. Update start/stop scripts for root-based launch.
6. Update CI workflow packaging and validations to match local behavior.
7. Update distribution README with new structure and troubleshooting.
8. Execute full build/test and packaged smoke test.

Rollback strategy:

- Restore previous copy strategy (`api/` and `web/` full outputs) in local and CI packaging.
- Revert start script directory assumptions to `api/` and `web/`.
- Revert packaged-mode config loading logic.
- Republish ZIP using previous layout.

## Implementation Results

Final validation run (local, `0.6.7-localtest`):

- Build/test:
  - `dotnet build -nologo` -> success (0 warnings, 0 errors).
  - `dotnet test -nologo` -> success (all test projects green).
- Packaged runtime smoke:
  - ZIP generated with `build-windows-dist.ps1`.
  - Extracted package started via `Start FamilyFinances.bat`.
  - API health confirmed: `http://localhost:5084/health` returned `200`.
  - Web confirmed reachable: `http://localhost:5019`.
  - Stop validated via `Stop FamilyFinances.bat`.
  - PID behavior validated: `api.pid`/`web.pid` created on start and removed on stop.
- Packaging-only scope check:
  - `CONTRACT_OR_MIGRATION_CHANGES=NONE` (no API/OpenAPI contract or migration file changes detected).
- ZIP size comparison (legacy `api/` + `web/` full trees vs shared-runtime layout):
  - Legacy ZIP bytes: `107,604,715`
  - New ZIP bytes: `57,099,745`
  - Saved bytes: `50,504,970`
  - Reduction: `46.94%`

## Open Questions

- Should non-tag CI artifacts also reduce retention days as part of this change, or remain unchanged?
- Should packaged merge emit a machine-readable manifest (e.g., JSON) for future regression tracking?

## IMPLEMENTATION VERIFICATION CHECKLIST

### A) Dependency convergence

- ? API and Web publish graphs are inspected for same-name collisions.
- ? Same-name conflicting binaries are reduced to zero (excluding intentionally separated config files).
- ? `Microsoft.Extensions.*` shared binaries resolve to one version across both outputs.
- ? `System.Text.Json` resolves to one version across both outputs.
- ? `System.IO.Pipelines` resolves to one version across both outputs.
- ? No accidental downgrade/upgrade is left implicit.
- ? `dotnet restore` succeeds for both projects.

### B) Packaging merge behavior

- ? API publish temp folder is produced.
- ? Web publish temp folder is produced.
- ? Merge copies non-existing files.
- ? Merge skips identical files by hash.
- ? Merge detects and reports hash-different collisions.
- ? Config collisions are redirected to app-specific config folders.
- ? Merge fails on non-config unresolved collisions.
- ? Final ZIP is created only after validations pass.

### C) Final distribution structure

- ? Root contains both `FamilyFinances.Api.exe` and `FamilyFinances.Web.exe`.
- ? Root contains one shared copy of identical runtime DLLs.
- ? `config/api` contains API appsettings and web.config.
- ? `config/web` contains Web appsettings and web.config.
- ? `wwwroot` is present for Web static assets.
- ? `data` directory exists in dist.
- ? `logs` directory exists in dist.
- ? Start/Stop scripts remain at root.

### D) Runtime behavior in packaged mode

- ? API starts successfully from root executable path.
- ? Web starts successfully from root executable path.
- ? API reads packaged config from `config/api`.
- ? Web reads packaged config from `config/web`.
- ? API health endpoint responds within script timeout.
- ? Web opens at `http://localhost:5019`.
- ? DB file is created/used under `data/`.
- ? API log file is created under `logs/`.

### E) Script and operations

- ? Start script still creates missing `data/` and `logs/`.
- ? Start script still opens browser automatically.
- ? Stop script still stops both processes by executable name.
- ? PID files are still written and cleaned.
- ? Error messaging remains operator-friendly.
- ? No hardcoded references to removed `api/`/`web/` runtime folders remain.
- ? README troubleshooting paths match new layout.

### F) CI parity

- ? CI produces same structural layout as local builder.
- ? CI verifies presence of both executables in root.
- ? CI verifies config folders and required appsettings files.
- ? CI fails on unresolved collisions.
- ? CI ZIP naming convention remains unchanged.
- ? CI artifact upload step still succeeds.
- ? Release upload step (tag builds) still succeeds.

### G) Regression and safety

- ? `dotnet build -nologo` succeeds after changes.
- ? `dotnet test -nologo` succeeds after changes.
- ? No API contract files are changed.
- ? No EF migrations are added.
- ? No business logic files changed for packaging-only scope unless required for config loading.
- ? Local smoke test validates start -> login page -> stop cycle.

### H) Documentation quality

- ? Distribution README reflects new folder layout exactly.
- ? Proposal/design/spec/tasks remain mutually consistent.
- ? All documentation text is English.
- ? Rollback steps are concrete and executable.
- ? Open questions are either resolved or tracked before implementation start.
