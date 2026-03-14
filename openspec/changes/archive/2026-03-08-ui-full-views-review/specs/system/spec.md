## ADDED Requirements

### Requirement: Web UI SHALL Enforce Shared Action Button And Accordion Primitives
The Web UI MUST use shared primitives for action buttons and grouped account sections to ensure consistent presentation across all major views.

#### Scenario: Export actions match standard application button shape
- **WHEN** any dashboard or report panel renders an `Export` action
- **THEN** the button MUST use the same rectangular geometry and corner radius used by standard primary/secondary buttons
- **AND** export controls MUST NOT render with isolated rounded-pill variants in those surfaces

#### Scenario: Grouped account lists reuse one accordion behavior contract
- **WHEN** grouped account sections are rendered in Accounts and Quick Entry
- **THEN** both views MUST use one shared single-open accordion behavior contract
- **AND** opening one section MUST close any other open section in the same accordion scope

### Requirement: Transaction Detail Navigation SHALL Preserve Origin Context
Transaction detail and edit flows MUST preserve the user origin context and route back to the originating surface.

#### Scenario: Account movements origin returns to the same account movements view
- **WHEN** a user opens transaction detail from `/accounts/{accountId}/movements`
- **THEN** detail and edit routes MUST carry whitelisted origin metadata
- **AND** back navigation from detail/edit MUST return to `/accounts/{accountId}/movements`

#### Scenario: Historical origin returns to the same historical surface
- **WHEN** a user opens transaction detail from historical transactions or historical movements views
- **THEN** detail/edit routes MUST preserve the originating historical route token
- **AND** back navigation MUST return to the same historical tab and filter context

#### Scenario: Report drilldown origin remains deterministic
- **WHEN** a user opens account movements from a report row drilldown and then opens transaction detail/edit
- **THEN** origin metadata MUST remain whitelisted and deterministic
- **AND** back navigation MUST return to the originating report-driven movement context

### Requirement: Payee Management UI SHALL Provide Searchable Card Presentation
The payees surface MUST provide a searchable responsive card grid while preserving existing rename and delete operations.

#### Scenario: Payees render as responsive cards with actions
- **WHEN** a user opens `/payees`
- **THEN** payees MUST render as cards with visible name, edit action, and delete action
- **AND** cards MUST wrap across rows responsively without forcing a single long vertical list

#### Scenario: Search continues to filter payees in card layout
- **WHEN** a user enters text in the payee search field
- **THEN** the rendered card set MUST be filtered by payee name matching behavior
- **AND** rename and delete actions MUST remain available on filtered results

### Requirement: Transactions List SHALL Show Payee As A Dedicated Column
The operational transactions list MUST expose payee information in its own table column and keep description semantics clean.

#### Scenario: Payee column is visible in transactions table
- **WHEN** a user opens `/transactions`
- **THEN** the table MUST include a dedicated `Payee` column
- **AND** payee text MUST no longer be duplicated as a prefix/suffix in the description cell

#### Scenario: Transactions search remains payee-aware
- **WHEN** a user searches transactions by text
- **THEN** filtering MUST continue to match payee values and description values
- **AND** result ordering and mutation actions MUST remain unchanged

### Requirement: Totals Reports SHALL Support Sorting And Account Drilldown
Totals report tables MUST support deterministic sorting and account-movement drilldown where requested.

#### Scenario: Category totals supports sortable columns
- **WHEN** a user opens category totals report results
- **THEN** defined sortable columns MUST toggle ascending and descending order deterministically
- **AND** sorting MUST not break existing totals semantics

#### Scenario: Category totals row opens account movements
- **WHEN** a user clicks an account row in category totals
- **THEN** the app MUST navigate to `/accounts/{accountId}/movements`
- **AND** drilldown navigation MUST include report period context metadata

#### Scenario: Account totals row opens account movements
- **WHEN** a user clicks an account row in account totals report
- **THEN** the app MUST navigate to `/accounts/{accountId}/movements`
- **AND** drilldown navigation MUST include report period context metadata

### Requirement: Login UI SHALL Remember Last Successful Username
The login form MUST remember and prefill the last successful username without persisting secrets.

#### Scenario: Last successful username is stored after successful login
- **WHEN** a login attempt succeeds
- **THEN** the submitted username/login identifier MUST be persisted in browser local storage for future prefill
- **AND** password values MUST NOT be persisted

#### Scenario: Login input prefills from stored username
- **WHEN** a user later opens `/login`
- **THEN** the username field MUST prefill with the stored last-successful value when available
- **AND** the password field MUST remain empty
