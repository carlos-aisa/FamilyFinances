## DESIGN: dashboard-ytd-net-kpi

## Scope
Add a fifth KPI card (`YTD Net`) in Dashboard and compute it as:

- current asset total (`DashboardOverview.AssetTotal.ValueCents`)
- minus asset total at previous year end (`31/12`) from `asset-total-balance` endpoint

This keeps backend unchanged (reuses existing endpoints) and adds no schema changes.

## Final Behavior

### KPI semantics
- `Income`, `Expense`, `Net Result`, `Net Worth`: unchanged.
- `YTD Net` value: `currentAssetTotal - previousYearEndAssetTotal`.
- `YTD Net` delta: `AssetTotal.DeltaVsPreviousMonthCents`.

### UI behavior
- KPI order: Income, Expense, Net Result, Net Worth, YTD Net.
- YTD card style: `border-warning`, same card structure as other KPIs.
- Breakpoints:
  - `<768px`: stacked (one card per row).
  - `768-1199px`: `col-md-6` gives 2-2-1.
  - `>=1200px`: `col-xl` equal-width distribution, no horizontal overflow.

### Localization
- `Dashboard_Kpi_YtdNet` in `SharedResource.resx`: `YTD Net`.
- `Dashboard_Kpi_YtdNet` in `SharedResource.es-ES.resx`: `Neto YTD`.

## Data Flow
1. Load dashboard overview (`/api/v1/reports/dashboard-overview`).
2. Compute `previousYearEnd = new DateOnly(overview.AsOf.Year - 1, 12, 31)`.
3. Load previous year-end asset total (`/api/v1/reports/asset-total-balance?asOf=...`).
4. Set:
   - `_ytdNetValue = overview.AssetTotal.ValueCents - previousYearAssetTotal.TotalCents`
   - `_ytdNetDelta = overview.AssetTotal.DeltaVsPreviousMonthCents`
5. Render YTD card in KPI strip.

## Key Decisions

### Decision 1: Keep computation in Web layer
Rationale:
- No new API contract.
- Reuses already exposed endpoint and DTOs.
- Fast to ship and easy to test at component level.

### Decision 2: Use stock-based YTD interpretation
Rationale:
- Product expectation validated in manual review: difference between current assets and 31/12 previous year.
- Better aligned with "how much I have now vs start of year" interpretation.

### Decision 3: Keep Bootstrap native responsive grid
Rationale:
- Avoid custom CSS complexity.
- Preserve consistency with existing dashboard cards.

## Risks and Mitigations
- Risk: previous-year asset request fails.
  - Mitigation: keep existing page load behavior (no crash); KPI keeps safe default values.
- Risk: semantic confusion between flow-YTD and stock-YTD.
  - Mitigation: spec and proposal updated to stock-based formula explicitly.

## IMPLEMENTATION VERIFICATION CHECKLIST

### Frontend
- [x] YTD card added in dashboard KPI strip after Net Worth.
- [x] YTD card uses `border-warning` + standard KPI card structure.
- [x] Label uses `Dashboard_Kpi_YtdNet` localization key.
- [x] Value uses signed money formatter and semantic color class.
- [x] Delta uses signed money formatter and semantic color class.

### Calculation
- [x] Previous year-end date derived from dashboard `AsOf` year.
- [x] Baseline requested via existing `GetAssetTotalBalanceAsync(asOf)`.
- [x] YTD value computed as current asset total minus previous year-end asset total.
- [x] YTD delta taken from `AssetTotal.DeltaVsPreviousMonthCents`.

### Layout
- [x] No horizontal overflow introduced by fifth KPI.
- [x] Small breakpoint stacks cards vertically.
- [x] Medium breakpoint keeps 2-2-1 behavior.
- [x] XL breakpoint keeps cards visible with equal-width distribution.

### Localization
- [x] English key/value added.
- [x] Spanish key/value added.
- [x] Existing localization behavior preserved.

### Tests
- [x] Dashboard component test validates fifth KPI rendering.
- [x] Dashboard component test validates warning border.
- [x] Dashboard component test validates YTD value shown from baseline formula.
- [x] ReportsApi tests cover `GetAssetTotalBalanceAsync` request and error paths.
- [x] Integration tests for dashboard overview payload remain green.

### Validation
- [x] `dotnet test --logger "console;verbosity=normal"` passes.
- [x] `dotnet build --configuration Release` passes.
- [x] Manual login/dashboard verification completed by user.
- [x] No blocking OpenSpec task remains.
