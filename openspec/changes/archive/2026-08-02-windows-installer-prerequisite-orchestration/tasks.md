## 1. Prerequisite Convergence Flow

- [x] 1.1 Update the supported Windows setup flow so IIS feature enablement happens before final `AspNetCoreModuleV2` validation and before IIS site/service provisioning begins.
- [x] 1.2 Add a deterministic install/repair step for the .NET Hosting Bundle when IIS becomes available during the same install run and `AspNetCoreModuleV2` is still missing.
- [x] 1.3 Ensure prerequisite orchestration remains idempotent across fresh install, retry, repair, and upgrade paths.

## 2. Installer Script And Bootstrapper Hardening

- [x] 2.1 Refactor prerequisite scripts/bootstrapper handoff so missing IIS features, missing `AspNetCoreModuleV2`, repair-needed, and reboot-required states are represented explicitly.
- [x] 2.2 Prevent MSI provisioning steps from running when prerequisite convergence has not completed successfully.
- [x] 2.3 Emit actionable installer diagnostics for clean-host failures, including guidance for rerun-after-reboot scenarios.

## 3. Packaging And Artifact Guidance

- [x] 3.1 Update installer packaging/build logic as needed so the supported `*-setup.exe` path can execute the revised prerequisite convergence behavior reliably.
- [x] 3.2 Preserve raw MSI publication while keeping it documented as the advanced/manual path unless prerequisite automation is intentionally extended to it.
- [x] 3.3 Verify artifact naming/output expectations remain unchanged for the Windows release workflow after prerequisite changes.

## 4. Validation

- [x] 4.1 Add focused automated validation for prerequisite-state detection and convergence decision points where practical in the existing toolchain.
- [x] 4.2 Execute installer smoke validation on a clean Windows host that starts without IIS and without `AspNetCoreModuleV2` registered. Requires a dedicated clean-host environment. Validation completion was confirmed by the user on 2026-08-02; no clean-host execution log is retained in this repository.
- [x] 4.3 Validate failure messaging for restart-required or unrecoverable prerequisite states so logs and user guidance remain actionable.

## 5. Documentation

- [x] 5.1 Update `tools/installer/windows/README.md` to clarify the supported setup path and prerequisite behavior.
- [x] 5.2 Update `docs/windows-distribution-build.md` with clean-host install behavior, restart guidance, and troubleshooting for Hosting Bundle repair cases.
- [x] 5.3 Update top-level install guidance if needed so end users are steered toward `*-setup.exe` for one-click installation.

## 6. Final Checks

- [x] 6.1 Run the affected installer/build validation commands and record results for the change. Executed `build-installer.ps1 -Version 1.5.2 -Configuration Release` successfully on 2026-07-05.
- [x] 6.2 Re-run any affected automated tests and confirm they pass for the modified installer behavior. Executed `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj --filter "FullyQualifiedName~Features.Installer"` successfully on 2026-07-05.
- [x] 6.3 Run `openspec validate windows-installer-prerequisite-orchestration --strict` and resolve all validation issues before implementation starts.
