## ADDED Requirements

### Requirement: Developer Workflow SHALL Support Optional External Skill Orchestration Without Runtime Coupling
The developer workflow system MUST support optional external-skill orchestration in a way that does not modify runtime application behavior.

#### Scenario: Workflow integration remains tooling-only
- **WHEN** OpenSpec+gstack orchestration is enabled
- **THEN** orchestration effects MUST be limited to planning/implementation/verification workflow artifacts and outputs
- **AND** runtime API/database/UI contracts MUST remain unchanged unless an explicit product change requires otherwise

#### Scenario: Tooling fallback is deterministic
- **WHEN** external orchestration cannot run
- **THEN** workflow execution MUST remain deterministic in OpenSpec-only mode
- **AND** users MUST receive clear fallback messaging without blocking normal OpenSpec flows
