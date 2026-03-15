## ADDED Requirements

### Requirement: Installed Runtime SHALL Default to Local-Only Exposure
A fresh installed deployment MUST default to local-only access and MUST NOT expose FamilyFinances endpoints to LAN until explicit opt-in is performed.

#### Scenario: Fresh install has no LAN exposure
- **WHEN** installation completes and user has not enabled LAN access
- **THEN** no LAN-facing HTTPS binding MUST be active for the Web host
- **AND** no inbound firewall allow rule for FamilyFinances LAN access MUST be present

#### Scenario: Local entrypoint remains available by default
- **WHEN** installed runtime is in default mode
- **THEN** users on the same machine MUST be able to access the application through local endpoints
- **AND** API traffic MUST remain internal to local host components

### Requirement: LAN Access SHALL Be Explicitly Opt-In and HTTPS-Only
When LAN mode is enabled from settings, exposure MUST be HTTPS-only and MUST be limited to private-network profiles.

#### Scenario: Enabling LAN mode creates HTTPS endpoint and private firewall rule
- **WHEN** an authorized user enables LAN access in settings
- **THEN** the host MUST configure an IIS HTTPS binding for the configured LAN endpoint
- **AND** the host MUST create only private-profile firewall allow rules for the configured HTTPS port

#### Scenario: Disabling LAN mode removes exposure controls
- **WHEN** an authorized user disables LAN access in settings
- **THEN** LAN HTTPS binding MUST be removed or disabled
- **AND** associated private-profile firewall allow rules MUST be removed

### Requirement: API SHALL Remain Loopback-Only In All Exposure Modes
The API runtime MUST remain loopback-bound and MUST NOT become directly reachable from LAN or public interfaces.

#### Scenario: API remains non-public when LAN mode is enabled
- **WHEN** LAN access is enabled for the Web host
- **THEN** API network bindings MUST remain restricted to loopback addresses
- **AND** firewall configuration MUST NOT expose API service ports

#### Scenario: Web-to-API communication remains internal
- **WHEN** installed runtime handles user requests in local or LAN mode
- **THEN** Web-to-API requests MUST resolve through local host communication
- **AND** no direct API external endpoint MUST be required for normal operation

### Requirement: Certificate Management SHALL Be Local and Deterministic
LAN TLS certificates MUST be generated and managed locally without external certificate dependencies.

#### Scenario: Local CA and server certificate are generated without internet dependency
- **WHEN** LAN certificate material is required for install or LAN enablement
- **THEN** the system MUST generate required certificate material locally
- **AND** certificate provisioning MUST NOT require external CA, DNS, or cloud service calls

#### Scenario: Certificate rotation updates active IIS binding
- **WHEN** an authorized user requests certificate regeneration
- **THEN** a new server certificate MUST be generated and bound to the configured IIS HTTPS endpoint
- **AND** runtime access MUST continue with the new certificate after operation completion

### Requirement: LAN Host Operations SHALL Be Policy Protected
Host-level operations that alter LAN exposure, certificates, or firewall state MUST be restricted to authorized users and auditable.

#### Scenario: Unauthorized actor cannot perform LAN host operations
- **WHEN** an unauthenticated or unauthorized actor attempts LAN host operations
- **THEN** the system MUST deny the operation
- **AND** no host networking/certificate state change MUST occur

#### Scenario: Authorized operation creates audit evidence
- **WHEN** an authorized actor performs LAN enable/disable or certificate rotation
- **THEN** the system MUST log operation outcome with timestamp and actor context
- **AND** sensitive secrets/private key material MUST NOT be written to logs
