## Context

The current Windows delivery model is a self-contained ZIP that users start and stop via `Start FamilyFinances.bat` and `Stop FamilyFinances.bat`. This model is functional for technical users but creates friction for home users, does not provide a native install/uninstall lifecycle, and relies on manual runtime orchestration.

Current runtime behavior relevant to this change:
- API and Web are currently started as standalone executables on `localhost` ports (`5084` and `5019`).
- Production configuration is loaded through packaged config roots (`FF_CONFIG_ROOT`) and environment overrides.
- API initialization performs migrations and identity seeding at startup.
- Release automation publishes installer assets (`setup.exe` + `msi`) and applies storage retention controls.

This change introduces cross-cutting deployment behavior (installer, IIS hosting, Windows services, local PKI, firewall orchestration, release artifact changes), so a design is required before implementation.

Stakeholders:
- Home users expecting one-time install and always-on behavior.
- Maintainers operating Windows release automation and support troubleshooting.
- Security/operations concerns for local-only default and LAN opt-in hardening.

## Goals / Non-Goals

**Goals:**
- Deliver an installer-first Windows deployment experience for home PCs.
- Host `FamilyFinances.Web` in IIS and run `FamilyFinances.Api` as an auto-start Windows Service.
- Enforce secure default exposure (`local-only`) and provide explicit LAN opt-in.
- Support LAN mode with HTTPS-only bindings, private-profile firewall rules, and API non-exposure.
- Generate per-install runtime secrets (especially JWT signing key) and local certificate material without external providers.
- Preserve rollback safety through installer recovery procedures and deterministic release governance.

**Non-Goals:**
- Implementing full role/permission redesign for settings administration.
- Supporting public internet/WAN publishing scenarios.
- Building cross-platform installer support (Linux/macOS).
- Changing business/domain behavior, API functional contracts, or reporting semantics.

## IMPLEMENTATION RULES - DO NOT DEVIATE

- [MUST] Keep API network binding loopback-only in all modes.
- [MUST] Default installation mode to local-only access.
- [MUST] Require elevated privileges for installation and privileged host changes.
- [MUST] Generate and persist a non-default JWT key during install/upgrade if missing.
- [MUST] Keep deployment changes inside Presentation/Infrastructure boundaries without Domain/Application leakage.
- [MUST] Keep release retention controls (historical artifacts cleanup) operational.
- [MUST NOT] expose API service ports through firewall rules.
- [MUST NOT] require any external CA, DNS, cloud secret manager, or third-party online service.
- [MUST NOT] reintroduce runtime ZIP publication in the active release flow.
- [MUST NOT] store private keys in world-readable filesystem paths.

## DETAILED UI FLOWS

### Flow 1: Fresh install (default local-only)
1. User launches installer with elevation consent.
2. Installer validates prerequisites and enables IIS Windows features if needed.
3. Installer deploys Web payload and API payload into managed install directories.
4. Installer registers API as Windows Service (`Automatic` startup).
5. Installer configures IIS site and app pool with local-only binding.
6. Installer writes runtime config in managed config root and generates JWT key if absent.
7. Installer starts API service and IIS site, validates health endpoints, and completes installation.

### Flow 2: Enable LAN mode from Settings
1. Authenticated user opens `/settings`.
2. User enables "LAN access" toggle and confirms security notice.
3. Backend privileged host operations apply:
   - create or rotate IIS HTTPS certificate if required,
   - bind HTTPS endpoint for LAN host/IP,
   - add private-profile firewall allow rule for configured HTTPS port.
4. API remains loopback-only.
5. Settings UI shows LAN mode active plus certificate trust guidance for mobile devices.

### Flow 3: Disable LAN mode
1. User disables "LAN access" in `/settings`.
2. Privileged operation removes LAN HTTPS binding and associated firewall allow rule.
3. Local-only path remains available from the same PC.

### Flow 4: Regenerate LAN certificate
1. User selects "Regenerate LAN certificate".
2. Privileged operation creates a new leaf certificate signed by local root CA.
3. IIS binding updates atomically to the new thumbprint.
4. UI warns users that mobile trust/import might need refresh.

## DETAILED PAGE WIREFRAMES

### Settings network panel

```text
+----------------------------------------------------------------------------------+
| Settings                                                                         |
+----------------------------------------------------------------------------------+
| Network Access                                                                   |
| Default mode: Local-only                                                         |
|                                                                                  |
| [ ] Enable access from local network (LAN)                                       |
|     HTTPS only. API remains local-only.                                          |
|                                                                                  |
| LAN URL: https://<pc-name-or-ip>:5443                                            |
| Certificate: Installed (thumbprint: XXXXX...)                                    |
|                                                                                  |
| [Regenerate certificate]   [Disable LAN access]                                  |
|                                                                                  |
| Mobile setup help                                                                 |
| 1) Download root certificate                                                     |
| 2) Install on iOS/Android                                                        |
| 3) Trust certificate profile                                                     |
+----------------------------------------------------------------------------------+
```

## COMPONENT REUSE MATRIX

| Area | Reuse | Modify | New |
|---|---|---|---|
| Packaging scripts | `build-windows-dist.ps1` semantics for deterministic publish | Release pipeline to publish installer-first | Installer build script/project |
| Deployment config bootstrap | `PackagedConfiguration` in API/Web | Add installer-managed config root conventions | Installer config templates/secrets generator |
| Settings UX | Existing `/settings` page cards and localization | Add Network/LAN panel and actions | LAN management UI state/actions |
| Security/auth policies | Existing JWT auth and admin role patterns | Enforce admin-only LAN host operations | Privileged host operation endpoint/service |
| Operational docs | Existing distribution/release docs | Update to installer lifecycle | New install/LAN/mobile trust guide |

## Decisions

### Decision 1: Use installer-first artifact with MSI-based deployment
- **Choice:** Produce an MSI-based installer (via WiX toolchain) as primary release artifact.
- **Rationale:** Native Windows install/upgrade/uninstall behavior, deterministic CI packaging, and better supportability than script-only distribution.
- **Alternatives considered:**
  - Keep ZIP + scripts as primary.
    - Rejected: still manual and not user-friendly for home scenario.
  - MSIX only.
    - Rejected: packaging constraints and service/IIS orchestration complexity for this workload.

### Decision 2: Host Web in IIS and API as Windows Service
- **Choice:** IIS hosts Web; API runs as dedicated Windows Service with automatic startup.
- **Rationale:** Keeps web entrypoint stable, isolates API process, and aligns with secure exposure model.
- **Alternatives considered:**
  - Host both Web and API in IIS.
    - Rejected: increases external surface and operational coupling.
  - Run both as standalone services on Kestrel.
    - Rejected: weaker hosting ergonomics for home-user install and harder HTTPS/firewall operations.

### Decision 3: Keep API loopback-only in all modes
- **Choice:** API binds only to `localhost/127.0.0.1`; never opened via firewall.
- **Rationale:** Minimizes attack surface and enforces Web as the only externally reachable entrypoint.
- **Alternatives considered:**
  - Expose API in LAN with token auth.
    - Rejected: larger attack surface and unnecessary for current architecture.

### Decision 4: Local PKI without external dependencies
- **Choice:** Generate local root CA and IIS server certificate on install; store in Windows certificate stores.
- **Rationale:** Meets offline/home requirement and provides HTTPS LAN support.
- **Alternatives considered:**
  - Public CA certificates.
    - Rejected: external dependency and domain ownership requirements.
  - Self-signed leaf only (no local root CA).
    - Rejected: poor rotation and trust management UX across devices.

### Decision 5: Installer-generated runtime secrets
- **Choice:** Generate install-specific JWT key and write to managed production config.
- **Rationale:** Removes static default secrets and hardens all deployments by default.
- **Alternatives considered:**
  - Keep template JWT key.
    - Rejected: unacceptable security baseline.

### Decision 6: Installer-only release distribution
- **Choice:** Publish installer assets only (`setup.exe`, `msi`) from release workflow.
- **Rationale:** Reduces operational ambiguity, simplifies support, and aligns with intended home-user UX.
- **Alternatives considered:**
  - Keep ZIP fallback published in parallel.
    - Rejected: increases support surface and contradicts installer-first operational policy.

### Decision 7: Privileged LAN host operations behind controlled server-side execution
- **Choice:** LAN enable/disable/certificate operations execute through privileged host orchestration (not direct client scripts).
- **Rationale:** Browser context cannot safely perform UAC/firewall/certificate operations; privileged execution must remain server-managed and auditable.
- **Alternatives considered:**
  - Client-side script downloads/manual execution.
    - Rejected: fragile UX and poor consistency.
  - Direct elevated API service identity for all runtime work.
    - Rejected: excessive privileges for request-handling process.

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: API service host bootstrap

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
// Keep existing packaged configuration + Serilog + infrastructure wiring.
```

### Example 2: Loopback-only API endpoint in production config

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://127.0.0.1:5084"
      }
    }
  }
}
```

### Example 3: LAN enable command contract

```json
{
  "enabled": true,
  "httpsPort": 5443,
  "hostName": "familyfinances.local",
  "regenerateCertificate": false
}
```

### Example 4: IIS HTTPS binding operation (conceptual)

```powershell
New-WebBinding -Name "FamilyFinances.Web" -Protocol https -Port 5443 -IPAddress "*" -HostHeader ""
New-Item "IIS:\SslBindings\0.0.0.0!5443" -Thumbprint $thumbprint -SSLFlags 0
```

## Risks / Trade-offs

- [Risk] Installer custom actions for IIS/service/certificates can fail on heterogeneous Windows setups.
  - Mitigation: idempotent prechecks, explicit failure diagnostics, and retry-safe custom actions.
- [Risk] LAN HTTPS trust on mobile requires manual root certificate trust steps.
  - Mitigation: guided in-app instructions and export of root certificate in user-friendly format.
- [Risk] Privileged host operations may be over-permissioned.
  - Mitigation: scope operations to a strict command set and keep runtime API process on least privilege identity.
- [Risk] Existing ZIP users may be disrupted by cutover.
  - Mitigation: provide explicit installer migration guidance and repair-focused troubleshooting.
- [Trade-off] Added operational complexity (IIS + service + certificates) vs script simplicity.
  - Mitigation: automate lifecycle end-to-end and include diagnostics tooling/documentation.
- [Trade-off] Local PKI improves autonomy but introduces trust onboarding friction on mobile devices.
  - Mitigation: keep local-only default and make LAN mode explicitly opt-in.

## Migration Plan

1. Add installer build pipeline and artifact publication in release workflow (installer-first).
2. Introduce API service-host readiness (`UseWindowsService`) and loopback-only production binding guarantees.
3. Add installer-managed filesystem layout:
   - binaries under Program Files,
   - mutable config/data/logs under ProgramData.
4. Implement secret generation on install/upgrade for missing or default JWT key.
5. Configure IIS web site/app pool and API service registration with automatic startup.
6. Add Settings network panel and backend LAN control contracts.
7. Implement privileged LAN operations:
   - HTTPS IIS binding management,
   - firewall private-profile rule management,
   - certificate generation/rotation.
8. Add tests:
   - unit tests for configuration/host operation orchestration,
   - integration tests for LAN settings API behavior and policy enforcement.
9. Update documentation (installation, LAN enablement, mobile trust, rollback).
10. Maintain installer-only release flow with pre-publish retention cleanup and operational troubleshooting guidance.

### Rollback Strategy

1. Pause automatic release publishing when installer pipeline is unstable.
2. Use installer repair/reinstall procedures and runtime-data preservation for recovery.
3. Revert LAN management endpoints/UI if privileged operation path is unstable.
4. Preserve existing CI quality/security workflows and release retention policies throughout rollback.

## Open Questions

- Which exact installer framework and version will be standardized in CI (WiX v4 vs v5 packaging profile)?
- Should installer upgrades always preserve existing certificates or rotate only on explicit user action?
- Do we need a dedicated local host-operations service, or can privileged tasks be safely delegated through installer-maintained scheduled tasks?
- What LAN hostname strategy should be default (`machine-name`, static alias, or explicit user input)?
- Should LAN toggle be restricted to Admin role immediately in this change or phased with the future settings-permissions change?

## IMPLEMENTATION VERIFICATION CHECKLIST

### Installer and runtime topology
- [ ] Installer requires elevation before privileged steps.
- [ ] IIS prerequisite checks run before deployment actions.
- [ ] Installer deploys Web and API payload to managed directories.
- [ ] API service is registered and set to automatic startup.
- [ ] API service starts successfully after install.
- [ ] IIS site is created and starts successfully.
- [ ] Default install mode is local-only.
- [ ] Uninstall stops API service and removes IIS site.
- [ ] Uninstall preserves user data by default.
- [ ] Optional full cleanup path removes data only after explicit confirmation.

### Security baseline
- [ ] API production bind is loopback-only.
- [ ] No firewall inbound rule is created for API port.
- [ ] JWT key is generated per install if missing/default.
- [ ] Generated JWT key length meets minimum security threshold.
- [ ] JWT key is stored in protected configuration location.
- [ ] Swagger UI remains disabled outside Development.
- [ ] Local-only mode has no LAN HTTPS binding.
- [ ] Local-only mode has no LAN firewall allow rule.
- [ ] Certificate private key is not stored in world-readable path.
- [ ] Sensitive operations are logged with redacted secrets.

### LAN mode behavior
- [ ] LAN mode can be enabled from Settings.
- [ ] LAN mode can be disabled from Settings.
- [ ] Enabling LAN creates HTTPS binding on configured port.
- [ ] Enabling LAN creates only private-profile firewall rule.
- [ ] Disabling LAN removes corresponding firewall rule.
- [ ] Disabling LAN removes LAN binding while preserving local access.
- [ ] API remains loopback-only when LAN is enabled.
- [ ] LAN mode status is persisted and correctly shown in UI.
- [ ] Certificate thumbprint/status is shown in Settings.
- [ ] Certificate regeneration updates IIS binding atomically.

### Certificate lifecycle
- [ ] Installer generates local root CA if missing.
- [ ] Installer generates server leaf certificate for IIS.
- [ ] Leaf certificate includes LAN-relevant SAN entries.
- [ ] Certificate validity periods are bounded and documented.
- [ ] Root certificate export path is documented for mobile trust.
- [ ] Certificate regeneration invalidates previous leaf binding.
- [ ] Existing root CA reuse behavior is deterministic.
- [ ] Certificate operations are idempotent.
- [ ] Certificate errors surface actionable diagnostics.
- [ ] Mobile trust instructions are present in docs/UI help.

### Settings and policy enforcement
- [ ] Settings page renders LAN controls for authorized users.
- [ ] Unauthorized users cannot call LAN host operation endpoints.
- [ ] API endpoints for LAN operations require authenticated policy.
- [ ] UI displays operation progress and terminal state.
- [ ] Failed LAN operations show user-friendly error messages.
- [ ] Settings labels/localization entries are added in English and Spanish resources.
- [ ] Existing theme/language/backup settings behavior is unchanged.
- [ ] Settings navigation remains stable across themes/densities.
- [ ] No secrets are rendered directly in UI.
- [ ] Operation audit entries include actor and timestamp.

### CI/CD and release governance
- [ ] Release workflow builds installer artifact on pushes to `main` with auto-version tagging.
- [ ] Release workflow still enforces artifact cleanup retention policy.
- [ ] Runtime ZIP is not published in active release flow.
- [ ] Installer artifact naming/versioning is deterministic.
- [ ] CI quality checks still pass on develop->main PR flow.
- [ ] Security workflows remain unaffected by installer changes.
- [ ] Publish job fails fast with clear diagnostics on installer build failure.
- [ ] Manual release validation checklist includes install/upgrade/uninstall.
- [ ] Release notes include migration and rollback guidance.
- [ ] Storage usage impact is monitored and documented.

### Testing and documentation
- [ ] Unit tests cover secret generation and config mutation safeguards.
- [ ] Unit tests cover LAN operation command validation.
- [ ] Integration tests cover protected LAN endpoints with auth policies.
- [ ] Integration tests verify local-only defaults after install config generation.
- [ ] Integration tests verify LAN enable/disable state transitions.
- [ ] Test runs remain deterministic and CI-safe.
- [ ] Documentation updated for installer installation flow.
- [ ] Documentation updated for LAN enablement and mobile trust.
- [ ] Documentation updated for installer-only rollback/support path.
- [ ] Troubleshooting guide includes IIS/service/certificate/firewall diagnostics.
