## 1. Localization

- [x] 1.1 Add `Dashboard_Kpi_YtdNet` in `src/FamilyFinances.Web/Resources/SharedResource.resx` with value `YTD Net`
- [x] 1.2 Add `Dashboard_Kpi_YtdNet` in `src/FamilyFinances.Web/Resources/SharedResource.es-ES.resx` with value `Neto YTD`
- [x] 1.3 Build Web project to validate resources compile

## 2. Dashboard KPI UI

- [x] 2.1 Add fifth KPI card (`YTD Net`) to `src/FamilyFinances.Web/Components/Pages/Dashboard/DashboardPage.razor`
- [x] 2.2 Keep card order: Income, Expense, Net Result, Net Worth, YTD Net
- [x] 2.3 Use warning visual style (`border-warning`) and existing KPI card structure
- [x] 2.4 Keep responsive classes compatible with existing strip (`col-12 col-md-6 col-xl`)

## 3. YTD Calculation Logic

- [x] 3.1 Add `_ytdNetValue` and `_ytdNetDelta` fields in dashboard component
- [x] 3.2 Add async YTD calculation step after overview load
- [x] 3.3 Query previous year-end baseline via `ReportsApi.GetAssetTotalBalanceAsync(asOf: 31/12 previous year)`
- [x] 3.4 Compute value as `overview.AssetTotal.ValueCents - previousYearEndAssetTotal.TotalCents`
- [x] 3.5 Set delta from `overview.AssetTotal.DeltaVsPreviousMonthCents`

## 4. API Client Reuse

- [x] 4.1 Reuse existing `GetAssetTotalBalanceAsync` in `ReportsApi` (no new endpoint)
- [x] 4.2 Keep auth/error handling behavior consistent with other reports methods

## 5. Tests

- [x] 5.1 Add/adjust dashboard component test for 5th KPI rendering and warning border
- [x] 5.2 Validate YTD value rendering against baseline formula in dashboard test
- [x] 5.3 Keep ReportsApi tests covering asset-total-balance request and error behavior
- [x] 5.4 Keep integration tests validating dashboard overview YTD payload contract

## 6. Manual Verification

- [x] 6.1 Start API in development mode
- [x] 6.2 Start Web in development mode
- [x] 6.3 Verify dashboard renders five KPI cards
- [x] 6.4 Verify YTD card label/value/delta display correctly
- [x] 6.5 Verify responsive behavior at small/medium/xl breakpoints

## 7. Documentation and Validation

- [x] 7.1 Update `README.md` dashboard KPI list to include `YTD Net`
- [x] 7.2 Add changelog entry for new KPI
- [x] 7.3 Run full tests: `dotnet test --logger "console;verbosity=normal"`
- [x] 7.4 Run release build: `dotnet build --configuration Release`
- [x] 7.5 Confirm design checklist reviewed and aligned with implementation
