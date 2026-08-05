## ADDED Requirements

### Requirement: Web UI SHALL Provide An Economic State As-Of-Date Report

The Web UI MUST provide a dedicated compact report that presents the economic state on an exact calendar date without changing the existing month-focused Economic State report.

#### Scenario: As-of-date report is reachable from reports index

- **WHEN** an authenticated user opens `/reports`
- **THEN** the UI MUST show an Economic State As-Of-Date report entry
- **AND** selecting it MUST navigate to the dedicated as-of-date report route

#### Scenario: Report defaults to today and disallows future dates

- **WHEN** the user opens the as-of-date report
- **THEN** its date input MUST default to the current local date
- **AND** its maximum selectable date MUST be the current local date
- **AND** the report MUST load that date without an additional user action

#### Scenario: Historical date renders exact stock and flow contexts

- **WHEN** the user applies a valid historical date `D`
- **THEN** the UI MUST request the existing economic-state read model with `asOf=D`
- **AND** it MUST display Assets, Liabilities, and Net Worth as balances as of `D`
- **AND** it MUST display Income, Expense, and Period Net Result for the inclusive period from the first day of `D`'s month through `D`

#### Scenario: Compact report excludes non-snapshot analysis

- **WHEN** the as-of-date report is rendered
- **THEN** it MUST NOT render evolution tabs, charts, monthly-history controls, or export controls
- **AND** the existing `/reports/economic-state` report MUST retain its current month-focused behavior
