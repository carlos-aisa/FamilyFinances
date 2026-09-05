## MODIFIED Requirements

### Requirement: Reporting UI SHALL Render Annual Evolution Charts for Implemented State-Evolution Scopes

The reporting UI MUST display annual charts only for implemented state-evolution scopes that retain a chart visualization (`asset-total`).

#### Scenario: Annual evolution chart is shown with selected year data

- **WHEN** an authenticated user opens an annual reporting view with charted evolution for year `Y`
- **THEN** the UI MUST show a chart visualizing monthly evolution across months `1..N` for that year
- **AND** chart points MUST use the same underlying monthly values shown in the corresponding table

## ADDED Requirements

### Requirement: Reporting UI SHALL Render Annual Account Group Evolution

The reporting UI MUST present annual account-group evolution on the Dashboard as a complete list of supplied groups. Account Group Totals State Evolution MUST retain its complete group summary on the left and use the right Evolution panel for the selected group's account detail. Each group row MUST show its signed selected-month amount and the UI MUST NOT hide groups through a presentation top-N cap.

#### Scenario: Group list shows every supplied group

- **WHEN** the user opens annual account-group evolution with group data
- **THEN** the Dashboard list and Account Group Totals summary MUST render one row per supplied group series
- **AND** rows MUST be ordered by absolute selected-month amount, then label and stable key
- **AND** every row MUST retain its source label and series identity

#### Scenario: Group nature uses semantic colour

- **WHEN** a group contains income accounts only
- **THEN** its monetary values MUST use the income semantic colour
- **WHEN** a group contains expense accounts only
- **THEN** its monetary values MUST use the expense semantic colour
- **WHEN** a group is mixed or its nature is unavailable
- **THEN** its monetary values MUST use a neutral colour

#### Scenario: Group row opens the matching monthly totals report

- **WHEN** a user follows a Dashboard group row
- **THEN** Account Group Totals MUST open for that group and the annual list's selected year and month
- **AND** the destination MUST use that calendar month as its report range

#### Scenario: Historical group month exposes its member accounts

- **WHEN** a user expands a group history and clicks one of its month rows
- **THEN** the right Evolution panel MUST show only the accounts that belong to that group
- **AND** each account MUST display its signed monthly and year-to-date value from the exact selected month
- **AND** the existing expand/collapse control MUST continue to only control history visibility
- **AND** the account detail rows MUST NOT navigate to account movements

### Requirement: Account Totals State Evolution SHALL Provide Account Composition

Account Totals State Evolution MUST show its expense and income composition analysis directly. It MUST NOT render the annual account evolution list or an Evolution/Composition mode selector.

#### Scenario: Composition is the only chart mode

- **WHEN** a user opens Account Totals State Evolution
- **THEN** the UI MUST render the composition analysis directly
- **AND** it MUST NOT render an annual account evolution list
- **AND** it MUST NOT offer an Evolution/Composition mode selector

#### Scenario: User selects composition nature and month

- **WHEN** a user selects Expense or Income and an available focused month
- **THEN** the composition chart MUST use the existing values for that nature and month

## REMOVED Requirements

### Requirement: Reporting UI SHALL Render Annual Account Group Evolution Chart
