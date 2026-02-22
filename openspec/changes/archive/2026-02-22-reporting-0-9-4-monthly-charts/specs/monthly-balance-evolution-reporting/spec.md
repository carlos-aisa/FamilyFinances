## MODIFIED Requirements

### Requirement: Web Reports UI SHALL Provide Integrated State Evolution Views
The Web UI MUST provide month-focused chart behavior inside integrated state-evolution tabs and MUST NOT require a dedicated Monthly Evolution route.

#### Scenario: Reports index keeps integrated entry points
- **WHEN** an authenticated user opens `/reports`
- **THEN** the UI MUST expose integrated report entries (`Economic State`, `Account Totals`, `Account Group Totals`)
- **AND** month-focused chart flows MUST be reachable through those report tabs without a standalone `/reports/monthly-evolution` entry

#### Scenario: User can select focused month in asset evolution tab
- **WHEN** the user opens `/reports/economic-state` and selects `Asset Evolution`
- **THEN** the view MUST provide focused-month controls for month-level charts
- **AND** changing month MUST reload month-level chart datasets for the selected year/month

#### Scenario: User can select focused month in account group state evolution tab
- **WHEN** the user opens `/reports/account-group-totals` and selects `State Evolution`
- **THEN** the view MUST provide focused-month controls for month-level charts
- **AND** changing month MUST reload month-level chart datasets for the selected year/month

#### Scenario: Month-focused chart and table context are consistent
- **WHEN** month-focused charts and summary rows are shown together in an integrated tab
- **THEN** both MUST reference the same selected month context
- **AND** labels MUST clearly indicate the selected month
