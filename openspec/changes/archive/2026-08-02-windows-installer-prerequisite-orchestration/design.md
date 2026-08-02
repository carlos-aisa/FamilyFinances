## Context

The current Windows installer flow publishes a setup bootstrapper plus a raw MSI. In the supported setup path, the bootstrapper first detects whether `AspNetCoreModuleV2` is present and, when it is missing, installs the .NET 9 Hosting Bundle. The MSI custom action then enables IIS features if needed, validates prerequisites, configures runtime files, registers the API Windows Service, creates the IIS site, and performs health checks.

That sequencing breaks on clean laptops that do not already have IIS enabled. Microsoft documents that when the Hosting Bundle is installed before IIS, the bundle must be repaired after IIS is installed so `AspNetCoreModuleV2` is registered correctly. The user-provided log from July 4, 2026 matches this failure pattern:
- `AspNetCoreModuleV2` is not detected at bootstrapper start.
- `DotNetHostingBundle90` installs successfully.
- `FamilyFinancesMsi` then fails with `0x80070643`.

The current setup also leaves the user with weak diagnostics because the top-level bundle log only exposes the generic MSI failure code. The same log shows `RebootPending=1`, so the revised flow must distinguish between "missing prerequisite", "repair required", and "restart required" states.

Constraints:
- Keep the existing installer-managed runtime topology (`FamilyFinances.Web` in IIS, `FamilyFinances.Api` as Windows Service).
- Stay within the current Windows installer toolchain (WiX + PowerShell orchestration).
- Keep raw MSI as an advanced/manual artifact unless the prerequisite payload is explicitly made available to it.
- Do not introduce new application-layer or API-layer behavior.

Stakeholders:
- Home users installing on personal Windows laptops and desktops.
- Maintainers building and validating the Windows release pipeline.
- Support/operations workflow that needs actionable installer diagnostics.

## Goals / Non-Goals

**Goals:**
- Make the supported `*-setup.exe` installation succeed on clean Windows hosts that start without IIS enabled.
- Ensure IIS feature activation and ASP.NET Core IIS module registration converge before IIS site provisioning begins.
- Provide deterministic repair/retry behavior when `AspNetCoreModuleV2` is still missing after IIS activation.
- Surface actionable diagnostics and restart guidance for prerequisite failures.
- Update validation and documentation so the clean-host scenario remains covered in future releases.

**Non-Goals:**
- Redesigning the installer UI or moving away from WiX/PowerShell orchestration.
- Changing LAN access, certificate handling, or API/network topology requirements.
- Making public internet hosting or non-Windows deployment part of this change.
- Refactoring unrelated runtime startup or business-domain code.

## Decisions

### Decision 1: Treat prerequisite convergence as a first-class install phase

Choice:
- The supported setup flow must complete a dedicated prerequisite convergence phase before the installer attempts IIS site creation or service registration.
- That phase must ensure required IIS Windows features are enabled and that final `AspNetCoreModuleV2` validation occurs only after IIS is available.

Rationale:
- The current failure is caused by provisioning work starting from an invalid prerequisite state.
- Converging prerequisites first keeps the rest of the MSI orchestration deterministic and easier to diagnose.

Alternatives considered:
- Keep the current order and only improve documentation: rejected because users still get a broken one-click installation path.
- Fail early and require manual IIS activation: rejected because the user requirement is that installer-managed setup should include those steps.

### Decision 2: Add Hosting Bundle self-healing after IIS activation

Choice:
- If IIS had to be enabled during setup and `AspNetCoreModuleV2` remains missing, the installer must run an install/repair step for the Hosting Bundle and re-check module registration before continuing.
- The supported setup path owns this repair behavior; it is not acceptable to require the user to rerun the installer manually just to repair module registration.

Rationale:
- On clean hosts, enabling IIS after an earlier Hosting Bundle install leaves IIS integration incomplete.
- An explicit repair/recheck step turns a fragile sequence into a self-healing one.

Alternatives considered:
- Depend on the user running setup twice: rejected because it preserves the broken experience.
- Assume health checks will eventually recover without repair: rejected because the module registration problem is structural, not transient.

### Decision 3: Stage Hosting Bundle payload in the MSI layout while keeping `*-setup.exe` as the supported entrypoint

Choice:
- The published setup bootstrapper remains the supported installation artifact for home-user scenarios where prerequisites may be missing.
- The MSI layout stages `installer-prereqs/dotnet-hosting-9.0-win.exe` so MSI prerequisite convergence can run Hosting Bundle maintenance locally after IIS activation.
- Raw MSI remains the advanced/manual artifact even though it now carries the same prerequisite payload for self-healing repair.

Rationale:
- Existing documentation already distinguishes setup bootstrapper from raw MSI usage.
- A staged local payload removes dependence on a second ad-hoc download during MSI prerequisite repair.
- This keeps scope tight while still fixing the real-world installation path users are expected to run.

Alternatives considered:
- Force MSI prerequisite repair to redownload the Hosting Bundle every time: rejected because it adds avoidable network dependency during repair.
- Remove the raw MSI artifact entirely: rejected because release packaging and advanced support workflows still use it.

### Decision 4: Surface explicit restart and prerequisite diagnostics

Choice:
- The installer must stop with clear diagnostics when prerequisite convergence cannot complete in the current session, especially when Windows reports a pending reboot state.
- Diagnostics must identify whether the failure is due to missing IIS features, missing `AspNetCoreModuleV2`, Hosting Bundle repair failure, or reboot-required state.

Rationale:
- The current top-level `0x80070643` is too generic for end users and maintainers.
- Clear failure categories reduce support time and make future logs actionable.

Alternatives considered:
- Keep generic MSI failure handling and rely on internal logs: rejected because it repeats the current support burden.

### Decision 5: Add clean-host validation as a required regression scenario

Choice:
- The change must add deterministic validation for the clean-host path (IIS absent, `AspNetCoreModuleV2` absent at start) and must include an installer smoke run on a Windows machine matching that state.
- Validation must also cover the diagnostic path for reboot-required or failed prerequisite convergence states.

Rationale:
- This defect escaped because the clean-machine path was not sufficiently guarded.
- The installer behavior is operationally critical and needs explicit regression coverage even if part of that coverage is smoke/manual.

Alternatives considered:
- Rely only on the existing release build checks: rejected because packaging success does not validate host prerequisite sequencing.

## Risks / Trade-offs

- [Risk] WiX/bootstrapper plus PowerShell sequencing changes can introduce new edge cases in upgrade or repair flows.
  -> Mitigation: keep the change scoped to prerequisite orchestration, preserve runtime provisioning steps, and validate install/repair/upgrade on Windows test hosts.

- [Risk] A pending reboot state can still block same-session convergence even with better sequencing.
  -> Mitigation: detect restart-required state explicitly and fail with a clear rerun instruction before site/service provisioning starts.

- [Risk] Keeping raw MSI as an advanced/manual path may still confuse users if docs are unclear.
  -> Mitigation: strengthen documentation to make `*-setup.exe` the clearly recommended path for clean-machine installs.

- [Trade-off] More prerequisite logic increases installer complexity.
  -> Mitigation: separate convergence from provisioning and keep each step idempotent with explicit diagnostics.

- [Trade-off] Additional smoke validation can lengthen release readiness checks.
  -> Mitigation: keep automated checks focused and reserve manual clean-host validation for release/installer changes only.

## Migration Plan

1. Refactor installer prerequisite handling so setup converges IIS features before final ASP.NET Core IIS module validation.
2. Add Hosting Bundle repair/recheck behavior for cases where IIS becomes available during the same install run.
3. Stop provisioning early with explicit diagnostics when convergence fails or Windows requires a reboot before continuing.
4. Update setup/raw MSI documentation and troubleshooting guidance to reflect the supported entrypoint and recovery steps.
5. Add or update validation assets for clean-host smoke testing and prerequisite-failure diagnostics.
6. Run OpenSpec validation and installer-focused verification before implementation completion.

Rollback strategy:
- Restore the prior bootstrapper/MSI prerequisite sequencing.
- Remove the new repair/recheck flow and revert documentation to manual IIS/Hosting Bundle recovery steps.
- If a released build must be rolled back, advise support to use the last known-good installer plus manual prerequisite activation guidance.

## Open Questions

- Do we want to hard-stop immediately when Windows reports a pre-existing pending reboot before setup begins, or only when prerequisite convergence itself creates a restart-required state?
- What is the minimal repeatable clean-host validation environment we want to standardize for future installer regressions (VM snapshot, dedicated laptop, or CI-hosted Windows image)?
