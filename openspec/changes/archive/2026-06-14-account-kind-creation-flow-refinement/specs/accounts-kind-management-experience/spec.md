## ADDED Requirements

### Requirement: Accounts Creation SHALL Keep Kind Selection Focused
The Accounts creation flow MUST keep `Kind` explicitly selectable while limiting the primary form to account-creation concerns.

#### Scenario: Account creation shows contextual kind selection
- **WHEN** an authenticated user opens the Accounts creation flow
- **THEN** the form MUST show a `Kind` selector
- **AND** the selector options MUST be limited to kinds compatible with the selected account `Nature`

#### Scenario: Full kind administration is not embedded by default
- **WHEN** an authenticated user views the primary account creation form
- **THEN** the form MUST NOT render the full custom-kind administration list by default
- **AND** low-frequency management actions MUST remain outside the main creation flow

### Requirement: Accounts Creation SHALL Support Inline Contextual Kind Creation
The Accounts creation flow MUST allow users to create a missing custom kind without leaving the form.

#### Scenario: Inline create form opens from the kind field
- **WHEN** the user invokes the contextual `New kind` action from the account creation form
- **THEN** the UI MUST reveal a compact inline create form associated with the `Kind` field
- **AND** the inline create form MUST remain visually subordinate to the main account form actions

#### Scenario: Inline kind creation inherits account nature
- **WHEN** the user submits a valid inline custom kind name from account creation
- **THEN** the system MUST create the custom kind with the same `Nature` currently selected for the account
- **AND** the user MUST NOT be required to select a second nature value inside the inline create form

#### Scenario: Created kind becomes immediately selectable and selected
- **WHEN** inline custom kind creation succeeds
- **THEN** the account form MUST refresh the compatible kind options
- **AND** the newly created custom kind MUST become the active selected kind for the account

#### Scenario: Inline create failure remains local
- **WHEN** inline custom kind creation fails validation or returns an application error
- **THEN** the error MUST be shown inside the inline create experience
- **AND** the main account form state MUST remain preserved

### Requirement: Accounts SHALL Provide A Secondary Kind Management Entry
The Accounts area MUST provide a secondary entry point for full kind administration outside the primary account creation form.

#### Scenario: Full kind management is reachable from Accounts
- **WHEN** an authenticated user needs low-frequency kind administration
- **THEN** the Accounts area MUST expose a distinct secondary `Manage kinds` entry
- **AND** that entry MUST open the surface responsible for full custom-kind administration

#### Scenario: Secondary management does not compete with primary create action
- **WHEN** the Accounts page renders both account-creation and kind-management entry points
- **THEN** the visual hierarchy MUST preserve `Create account` as the dominant action
- **AND** kind-management actions MUST be presented as secondary utilities

### Requirement: Accounts Kind Selection SHALL Recover From Nature Changes
The Accounts creation flow MUST keep the selected kind valid when the account nature changes.

#### Scenario: Nature change filters the available kinds
- **WHEN** the user changes the account `Nature` during account creation
- **THEN** the `Kind` selector MUST recompute its available options using the new nature
- **AND** only compatible system and custom kinds MUST remain selectable

#### Scenario: Invalid selected kind falls back to a valid default
- **WHEN** the currently selected kind is no longer compatible after a nature change
- **THEN** the form MUST replace it with the default compatible kind for the new nature
- **AND** the form MUST remain submittable without manual recovery steps
