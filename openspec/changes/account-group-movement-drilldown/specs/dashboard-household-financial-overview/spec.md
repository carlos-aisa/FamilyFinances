## MODIFIED Requirements

### Requirement: Dashboard SHALL Monitor User-Pinned Account Groups

The Dashboard MUST show only account groups explicitly selected for monitoring by the user.

#### Scenario: Pinned group rows show operational result

- **WHEN** one or more groups have `IsDashboardPinned = true`
- **THEN** the Dashboard MUST display each pinned group's current-month and YTD operational result
- **AND** each result MUST include only Income and Expense member accounts.

#### Scenario: Pinned group row links to its selected-period report

- **WHEN** a user activates a pinned group name in the Dashboard
- **THEN** the Dashboard MUST navigate to `/reports/account-group-totals`
- **AND** the route MUST include that group's identifier and the Dashboard selected year and month.

#### Scenario: Pinned groups order by current-month operational result

- **WHEN** pinned group rows are returned for the Dashboard
- **THEN** they MUST be ordered by monthly operational result ascending.

#### Scenario: Expense group metrics use neutral magnitude display

- **WHEN** a pinned group is classified as an Expense metric
- **THEN** its current-month and YTD values MUST be shown as non-negative magnitudes
- **AND** those values MUST NOT use unfavorable-result color semantics.

#### Scenario: Balance-account natures are excluded from group operational result

- **WHEN** a pinned group includes Asset, Liability, or Equity accounts
- **THEN** those accounts MUST NOT contribute to its dashboard operational result.

#### Scenario: Overlapping groups remain independent monitoring views

- **WHEN** one account belongs to multiple pinned groups
- **THEN** its eligible flow contribution MUST appear in each relevant group row
- **AND** the Dashboard MUST NOT show group percentages or an aggregate total across group rows.

#### Scenario: No pinned groups has an actionable empty state

- **WHEN** no group is pinned
- **THEN** the Dashboard MUST render a localized compact empty state
- **AND** it MUST direct the user to account-group management.
