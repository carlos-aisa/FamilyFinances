## Why

The dashboard shows period KPIs (Income, Expense, Net Result, Net Worth), but users also need a fast indicator of total asset progression in the current year.  
The chosen business metric is:

- current total assets
- minus total assets at **December 31st of the previous year**

This gives a practical "YTD Net" view aligned with real balance evolution.

## What Changes

- Add a fifth KPI card in dashboard top strip: `YTD Net`
- Compute YTD value in Web as:
  - `DashboardOverview.AssetTotal.ValueCents`
  - minus `AssetTotalBalance(asOf = 31/12 previous year).TotalCents`
- Keep monthly delta from `DashboardOverview.AssetTotal.DeltaVsPreviousMonthCents`
- Keep visual order after Net Worth and warning (yellow/orange) border style
- Keep responsive layout with 5 cards and no horizontal overflow

## Release Impact

Type: minor
Rationale: New backward-compatible functionality adding a KPI to existing dashboard without changing current behavior

## Capabilities

### New Capabilities
<!-- None - this modifies existing dashboard capability -->

### Modified Capabilities
- `dashboard-household-financial-overview`: Add YTD Net KPI to top strip, displaying asset variation versus previous year-end baseline

## Impact

**Backend:**
- No new endpoint introduced
- Reuse existing endpoints:
  - `GET /api/v1/reports/dashboard-overview`
  - `GET /api/v1/reports/asset-total-balance`

**Frontend:**
- Dashboard page component: add fifth KPI card
- Add YTD calculation against prior year-end asset baseline
- Keep dashboard layout consistent with 5 KPIs in strip
- Add localization keys for YTD Net labels

**Testing:**
- Update dashboard page tests for YTD card rendering/value
- Keep API integration tests validating YTD summary payload contract
