## ADDED Requirements

### Requirement: Reporting UI SHALL Support Export of Tabular Data
Reporting pages MUST provide CSV export for tabular report data.

#### Scenario: CSV export downloads filtered report data
- **WHEN** a user triggers CSV export from a report page with active filters
- **THEN** the exported CSV MUST reflect the currently filtered data context
- **AND** exported numeric values MUST match on-screen table values

### Requirement: Reporting UI SHALL Support Chart Image Export Where Charts Exist
Reporting pages with charts MUST provide chart image export behavior.

#### Scenario: Chart image export captures active chart state
- **WHEN** a user triggers chart export on a chart-enabled report page
- **THEN** the exported image MUST represent the currently visible chart series and filters

### Requirement: Reporting UI SHALL Meet Responsive and Accessibility Baselines
Reporting pages MUST satisfy responsive and accessibility baseline criteria.

#### Scenario: Mobile and desktop layouts remain usable
- **WHEN** report pages are rendered on supported viewport sizes
- **THEN** critical controls, KPI cards, charts, and tables MUST remain usable without layout breakage

#### Scenario: Accessibility baseline is satisfied
- **WHEN** reporting pages are validated with accessibility checks
- **THEN** key interactive controls MUST have accessible labels and keyboard operability

### Requirement: Reporting Release SHALL Enforce Final Regression Gates
Final `0.9` release MUST satisfy predefined reporting regression gates.

#### Scenario: Release gate fails on regression
- **WHEN** critical reporting regression tests fail
- **THEN** `0.9` closeout MUST be blocked until failures are resolved

