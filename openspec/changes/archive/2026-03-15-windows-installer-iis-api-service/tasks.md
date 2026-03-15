## 1. Installer Foundation and Packaging Skeleton

- [x] 1.1 Create installer project skeleton under `tools/installer/windows/` (WiX toolchain files, source layout, build entry script).
- [x] 1.2 Define managed install/runtime directories (binaries vs mutable config/data/logs) and document path contract in installer constants.
- [x] 1.3 Add installer precheck steps for elevation, IIS availability, and required runtime prerequisites.
- [x] 1.4 Add installer actions to deploy Web/API payload files into managed locations with deterministic versioned inputs.

## 2. API Service and Web Host Topology

- [x] 2.1 Update API host bootstrap in `src/FamilyFinances.Api/Program.cs` to support Windows Service hosting (`UseWindowsService`) without breaking non-installed execution.
- [x] 2.2 Add installer-driven API service registration/unregistration scripts under `tools/installer/windows/scripts/` with idempotent start/stop handling.
- [x] 2.3 Add installer-driven IIS site and app-pool provisioning scripts under `tools/installer/windows/scripts/` for `FamilyFinances.Web`.
- [x] 2.4 Ensure local-only default bindings for installed topology (Web local entrypoint and API loopback-only endpoint).
- [x] 2.5 Implement post-install health validation script (API health + Web reachability) and wire failure to installer error state.

## 3. Runtime Configuration and Secret Hardening

- [x] 3.1 Add installer config template generation for API/Web managed config roots (compatible with `FF_CONFIG_ROOT` packaged loading).
- [x] 3.2 Implement JWT key generation/validation logic in installer orchestration to replace missing/default secrets.
- [x] 3.3 Preserve existing non-default JWT keys during upgrade flows and add explicit optional rotation command path.
- [x] 3.4 Add tests for config mutation and secret generation helpers in `tests/` (unit tests, deterministic assertions, no secret leakage in logs).

## 4. LAN Access Control and Local PKI

- [x] 4.1 Add backend contracts/services for LAN host operations (enable/disable LAN, certificate regeneration, status query) in Web/API layers without violating architecture boundaries.
- [x] 4.2 Implement local certificate authority + server certificate generation/lookup logic for IIS bindings in installer/host operation scripts.
- [x] 4.3 Implement IIS HTTPS binding add/remove/update operations for LAN mode with idempotent behavior.
- [x] 4.4 Implement firewall private-profile rule add/remove operations scoped to configured Web HTTPS port only.
- [x] 4.5 Enforce API loopback-only guarantee for all LAN states and add guard checks preventing API firewall exposure.

## 5. Settings UI and Authorization for Host Operations

- [x] 5.1 Extend `src/FamilyFinances.Web/Components/Pages/Settings/SettingsPage.razor` with a new Network Access panel for LAN toggle/status/certificate actions.
- [x] 5.2 Add localized resource entries (EN/ES) for new LAN settings labels, warnings, status messages, and operation outcomes.
- [x] 5.3 Add protected endpoints/handlers for LAN host operations requiring authenticated authorized users.
- [x] 5.4 Add operation audit logging for LAN/security actions with actor/timestamp while redacting secrets/private key material from logs.
- [x] 5.5 Add mobile certificate trust guidance surface in settings/help content (download/export + installation steps).

## 6. Release Workflow and Distribution Transition

- [x] 6.1 Update `.github/workflows/release-windows.yml` to build and publish installer artifacts as primary release deliverables on `main` pushes with auto-version tagging.
- [x] 6.2 Remove ZIP artifact generation/publication from active release flow (installer-only policy).
- [x] 6.3 Preserve release asset cleanup policy (historical retention) and adapt matching rules for installer assets (plus legacy ZIP cleanup pattern).
- [x] 6.4 Add workflow validation checks for installer build output existence and fail-fast error reporting.
- [x] 6.5 Update local build script/docs to describe installer-only delivery.

## 7. Upgrade, Uninstall, and Rollback Safety

- [x] 7.1 Implement upgrade behavior that preserves existing user data/config by default and verifies IIS/service rebind success.
- [x] 7.2 Implement uninstall behavior that removes host registrations while preserving data by default.
- [x] 7.3 Implement explicit destructive cleanup option with clear confirmation semantics and documentation.
- [x] 7.4 Add rollback/support procedures for installer release issues without reintroducing ZIP as primary distribution.

## 8. Test Coverage and Validation Gates

- [x] 8.1 Add/extend unit tests for LAN command validation and host operation policy checks (authorized vs unauthorized behavior).
- [x] 8.2 Add integration tests for LAN settings endpoints and state transitions using deterministic test setup.
- [x] 8.3 Add tests validating default local-only exposure after installer config generation.
- [x] 8.4 Run and verify required test suites:
- [x] 8.5 `dotnet test tests/FamilyFinances.Api.IntegrationTests/FamilyFinances.Api.IntegrationTests.csproj -c Release`
- [x] 8.6 `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj -c Release`
- [x] 8.7 `dotnet test tests/FamilyFinances.Application.Tests/FamilyFinances.Application.Tests.csproj -c Release`
- [x] 8.8 Validate installer smoke checks on Windows test machine/runner (install -> reboot/startup check -> LAN toggle -> uninstall). (Validated manually on 2026-03-15)

## 9. Documentation and Final OpenSpec Validation

- [x] 9.1 Update `README.md` with installer-first installation, upgrade, uninstall, and local-only default behavior.
- [x] 9.2 Update `docs/windows-distribution-build.md` to cover installer artifact build/publish flow (installer-only).
- [x] 9.3 Add a dedicated operational guide for LAN enablement, certificate trust on mobile devices, and security caveats.
- [x] 9.4 Document rollback and support troubleshooting for IIS/service/certificate/firewall issues.
- [x] 9.5 Run `openspec validate --type change windows-installer-iis-api-service` and resolve all issues before implementation completion.



