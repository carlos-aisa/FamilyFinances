## CRITICAL IMPLEMENTATION CONSTRAINTS

### Forbidden
- Do not expose `FamilyFinances.Api` directly to LAN or public networks.
- Do not ship production installs with a static/default JWT key.
- Do not require external certificate providers or cloud services.
- Do not bypass layered architecture boundaries.

### Required
- Installer defaults to local-only access on first install.
- Installer runs elevated and configures IIS (Web) plus Windows Service (API).
- LAN access is opt-in, HTTPS-only, and limited to private network profile firewall rules.
- Release flow publishes installer-only assets with deterministic retention and auto-versioning on `main` pushes.

## Why

The current ZIP plus batch-script distribution works for technical users but is not practical for home users who expect a one-click installation and automatic startup behavior. We also need a safer default deployment model that keeps the system closed to external access unless explicitly enabled.

## What Changes

- Introduce a Windows installer as the primary home-user deployment artifact.
- Configure `FamilyFinances.Web` under IIS and `FamilyFinances.Api` as an auto-start Windows Service during installation.
- Generate install-specific runtime secrets (including JWT signing key) and local certificate assets for secure HTTPS bindings.
- Add an application setting to enable or disable LAN access, with secure defaults and controlled firewall changes.
- Update release automation to produce/publish installer artifacts and retain current storage-governance behavior.
- Remove ZIP publication from active release flow (installer-only distribution).
- **BREAKING**: primary release consumption moves from manual `Start/Stop` scripts to installed IIS/Service runtime management.

## Capabilities

### New Capabilities
- `windows-installer-managed-deployment`: Provide install/upgrade/uninstall lifecycle for home Windows PCs, including IIS site provisioning, API Windows Service registration, automatic startup, and post-install validation.
- `secure-lan-access-control`: Provide a settings-driven LAN access toggle with secure defaults, local PKI certificate handling, HTTPS-only exposure, and private-profile firewall enforcement.

### Modified Capabilities
- `windows-distribution-packaging`: Evolve packaging requirements from ZIP-first script execution to installer-only release delivery with deterministic governance.

## Impact

- Affected code areas:
  - Windows packaging scripts and release workflow definitions.
  - Web/API hosting bootstrap and production configuration handling.
  - New installer orchestration scripts/project and setup assets.
  - Settings surface and backend operations for LAN toggle and certificate lifecycle.
- Operational impact:
  - End users move from manual `.bat` startup to always-on installed runtime.
  - Deployment defaults become safer (local-only first, explicit LAN opt-in).
- Security impact:
  - Stronger network exposure control and unique per-install cryptographic material.
  - API remains non-public and reachable only from local host components.
- Documentation impact:
  - New installation, upgrade, uninstall, LAN enablement, and mobile certificate trust guides.

## Non-Goals

- Implementing role-based permissions for the Settings area in this change.
- Supporting internet/public WAN exposure scenarios.
- Introducing Linux/macOS installers or non-Windows hosting targets.
- Changing business-domain behavior, ledger rules, or report semantics.

## Rollback Plan

- If installer deployment is unstable, pause automatic release publish and remediate installer pipeline before next release.
- Use installer repair/reinstall flow as first-line recovery path.
- Preserve runtime data locations so users can recover safely across installer upgrades/uninstalls.
