# dashboard-household-financial-overview Delta Specification

## MODIFIED Requirements

### Requirement: Dashboard SHALL Provide Current-Month Versus Previous-Month Comparison Semantics
The Dashboard MUST present current-month values and previous-month deltas for core household indicators.

#### Scenario: KPI strip shows mandatory comparison metrics
- **WHEN** KPI data is available for the selected/current month
- **THEN** the Dashboard MUST display `Income`, `Expense`, `Net Result`, `Net Worth`, and `YTD Net`
- **AND** each KPI MUST include a deterministic delta against the previous month

#### Scenario: Net Result semantics are explicit
- **WHEN** the Dashboard renders period net values
- **THEN** net result MUST be computed as `Income - Expense`
- **AND** positive net result MUST indicate income exceeds expense

#### Scenario: YTD Net shows variation versus previous year-end baseline
- **WHEN** the Dashboard displays YTD Net KPI
- **THEN** the value MUST be computed as:
  - current `AssetTotal.ValueCents`
  - minus asset total at `31/12` of the previous year
- **AND** the delta MUST use the current month stock delta (`AssetTotal.DeltaVsPreviousMonthCents`)

#### Scenario: YTD Net is visually distinct from period KPIs
- **WHEN** the Dashboard renders the YTD Net KPI card
- **THEN** it MUST use a distinct border color (warning/yellow) to differentiate it from period-focused KPIs
- **AND** it MUST follow the same card structure as other KPIs (label, value, delta)
- **AND** it MUST display after the Net Worth KPI in the visual order

## ADDED Requirements

### Requirement: Dashboard SHALL Derive YTD Baseline Using Asset Total As-Of Endpoint
The Dashboard MUST derive YTD baseline from existing asset-total endpoint data, without introducing new backend APIs.

#### Scenario: Previous year-end asset total is queried using existing report endpoint
- **WHEN** YTD Net KPI is calculated
- **THEN** the client MUST request asset total balance for `asOf = 31/12` of previous year
- **AND** the request MUST use existing reports API client infrastructure

#### Scenario: YTD Net value uses stock baseline formula
- **WHEN** both current dashboard overview and previous year-end asset totals are available
- **THEN** YTD Net value MUST be `currentAssetTotal - previousYearEndAssetTotal`
- **AND** the UI MUST format value and delta with signed money formatter and semantic color classes

### Requirement: Dashboard KPI Layout SHALL Accommodate Five KPIs
The Dashboard KPI strip MUST scale to display five KPIs while maintaining responsive behavior and visual consistency.

#### Scenario: Five KPIs display without horizontal overflow
- **WHEN** the Dashboard renders the KPI strip with five KPIs
- **THEN** all five KPIs MUST be visible without horizontal scrolling at any supported breakpoint
- **AND** the layout MUST adapt gracefully from mobile (vertical stack) to desktop (horizontal layout)

#### Scenario: KPI responsive layout handles fifth KPI
- **WHEN** viewport is at small breakpoint (<768px)
- **THEN** all five KPIs MUST stack vertically (one per row)
- **WHEN** viewport is at medium breakpoint (768-1199px)
- **THEN** KPIs MUST display in a 2-2-1 layout (two rows of two, one row of one)
- **WHEN** viewport is at xl breakpoint (>=1200px)
- **THEN** all KPI cards MUST remain visible in the first viewport row/flow with equal-width distribution
- **AND** the layout MUST remain overflow-safe without requiring custom horizontal scrolling
