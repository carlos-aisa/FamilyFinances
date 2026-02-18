## Context

FamilyFinances currently provides point-in-time and period reports (`monthly-summary`, `category-totals`, `account-totals`, `account-group-totals`), but it does not expose a year-to-date monthly evolution model with explicit month-over-month and year-start deltas.

The new change must:
- Add a dedicated report experience (not an ad-hoc extension of existing reports).
- Support three report perspectives in the same feature: Accounts, Asset Total, and Account Groups.
- Provide a graph-ready response contract now, even if the first UI version renders tables/cards only.

Current technical constraints:
- Layered architecture must remain strict (`Presentation -> Application -> Domain`, `Infrastructure -> Application/Domain`).
- Existing reporting stack pattern is: `ReportsController -> Query/Handler -> IReportingReadRepository -> ReportingReadRepository`.
- Existing Web report stack pattern is: `ReportsApi` client + page under `src/FamilyFinances.Web/Components/Pages/Reports`.
- Existing fiscal-year snapshots already exist and should be leveraged where possible for performance.

Stakeholders:
- Primary: end users who want monthly balance evolution from January of a selected year.
- Secondary: maintainers who will add charts in future versions and need stable machine-friendly data contracts.

## Goals / Non-Goals

**Goals:**
- Provide a dedicated monthly YTD evolution report route in Web Reports.
- Support one-year monthly points (from January to selected-year end window) with:
  - `EndBalanceCents`
  - `DeltaVsPreviousMonthCents`
  - `DeltaVsYearStartCents`
- Expose three scopes from the same feature:
  - Accounts
  - Asset Total
  - Account Groups
- Ensure payload is graph-ready:
  - ordered points
  - stable series keys
  - machine-friendly numeric fields in cents
- Reuse existing reporting architecture and error-handling behavior.
- Keep monthly rows visible even if there were no movements in a month (carry-forward balance, zero delta when applicable).

**Non-Goals:**
- No chart rendering libraries or chart UI components in this change.
- No custom arbitrary date range (scope is selected `year`, month buckets from January).
- No transaction-level drilldown from evolution rows.
- No multi-currency conversion or currency contract changes.
- No temporal membership model for account groups in this change.

## IMPLEMENTATION RULES - DO NOT DEVIATE

### Required
- Use a dedicated report page route: `/reports/monthly-evolution`.
- Add one Reports index card entry for Monthly Evolution.
- Add one API entry point with explicit `year` and `scope` query parameters.
- Keep all evolution amounts in integer cents in API contracts.
- Keep one canonical point model for all three scopes to stay chart-ready.
- Implement deterministic month ordering (`Month` ascending).
- Keep delta semantics exact:
  - `DeltaVsPreviousMonthCents = EndBalance(month M) - EndBalance(month M-1)`
  - `DeltaVsYearStartCents = EndBalance(month M) - EndBalance(December previous year)`
- Include no-activity months in the selected year window.
- Add unit/integration/web tests for:
  - delta correctness
  - inclusive month boundaries
  - zero/no-data behavior
  - auth behavior and client deserialization

### Forbidden
- Do not piggyback on existing `account-totals` or `account-group-totals` contracts.
- Do not return localized strings from API (labels can be UI-level).
- Do not add chart-specific shape that breaks table-first rendering.
- Do not bypass `IReportingReadRepository` from controller/handler.
- Do not add InMemory EF integration tests.

## Goals / Non-Goals Decision Clarification

This design intentionally prefers a single canonical time-series contract over multiple shape-specific contracts. This reduces future migration cost when charts are introduced.

## Decisions

### Decision 1: API topology uses a single evolution endpoint with scope parameter

- **Choice:** Add `GET /api/v1/reports/monthly-evolution?year=YYYY&scope=<accounts|asset-total|account-groups>`.
- **Rationale:** One endpoint and one response shape simplify Web consumption and future chart components.
- **Alternative considered:** Three endpoints (`/accounts`, `/asset-total`, `/account-groups`).
  - **Rejected because:** duplicates contract and parser logic, increases doc/test surface, and makes chart reuse harder.

### Decision 2: Use one canonical graph-ready response shape

- **Choice:** Return report payload with `Series[]`, each containing ordered `Points[]` (monthly buckets).
- **Rationale:** A single series/point schema supports both tables now and charts later without contract break.
- **Alternative considered:** Flat tabular API rows only.
  - **Rejected because:** chart adaptation would require an API refactor or expensive client reshaping.

### Decision 3: Delta semantics anchored to prior month and previous-year close

- **Choice:** For selected year `Y`, define baseline as end balance at `Y-01-01` (equivalent to closing at `Y-1-12-31`).
- **Rationale:** This makes "vs year start" consistent and avoids ambiguity for January.
- **Alternative considered:** Baseline at first transaction in year.
  - **Rejected because:** different accounts/groups could have different baselines and inconsistent deltas.

### Decision 4: Selected-year month window

- **Choice:** Return months:
  - `1..currentMonth` when `year == currentYear`
  - `1..12` for past years
- **Rationale:** Avoid future-month placeholders while still covering full historical years.
- **Alternative considered:** Always `1..12`.
  - **Rejected because:** current year would expose future months with artificial carry-forward values.

### Decision 5: Include no-activity months

- **Choice:** Always include month buckets in the selected window; when no movements exist in month `M`, carry balance forward and produce zero month delta.
- **Rationale:** Continuous lines for future charts and complete monthly auditability.
- **Alternative considered:** Skip months with no changes.
  - **Rejected because:** discontinuous timeline harms analysis and chart rendering.

### Decision 6: Reuse account-level monthly aggregates as the base for all scopes

- **Choice:** Compute account monthly balances first; derive:
  - Asset Total by summing account series where `AccountNature == Asset`
  - Account Groups by summing account series belonging to each group
- **Rationale:** One canonical base dataset avoids divergence and keeps formula consistency.
- **Alternative considered:** Separate SQL aggregate paths per scope.
  - **Rejected because:** repeated logic and higher risk of inconsistent deltas.

### Decision 7: Performance path uses fiscal-year snapshots when available

- **Choice:** Use `AccountYearSnapshot` (`year = selectedYear - 1`) as balance baseline where present, fallback to historical sum when missing.
- **Rationale:** Keeps runtime predictable for multi-year ledgers while preserving correctness.
- **Alternative considered:** Always sum full history from origin.
  - **Rejected because:** unnecessary cost at scale.

### Decision 8: Web UI is tab-first with table-first rendering

- **Choice:** New page with three tabs (`Accounts`, `Asset Total`, `Account Groups`) and year selector; render table data in all tabs.
- **Rationale:** Meets immediate requirement and keeps UX ready for future charts.
- **Alternative considered:** Extend each existing report page separately.
  - **Rejected because:** fragmented UX and duplicated controls.

## DETAILED UI FLOWS

### Flow 1: Navigate to Monthly Evolution report
1. User opens `/reports`.
2. User clicks `Monthly Evolution` report card.
3. App navigates to `/reports/monthly-evolution`.
4. Page defaults:
   - `Year = current year`
   - `Scope tab = Asset Total` (lightweight first view)
5. Page loads dataset for selected `year` and `scope`.

### Flow 2: Change year
1. User selects year `Y` from year dropdown.
2. UI clears previous error and enters loading state.
3. UI calls `ReportsApi.GetMonthlyEvolutionAsync(Y, activeScope)`.
4. On success:
   - table updates with monthly points and deltas
   - summary cards update
5. On failure:
   - error banner shown
   - previous successful data remains cleared to avoid stale interpretation

### Flow 3: Switch scope tab
1. User selects one of tabs:
   - `Accounts`
   - `Asset Total`
   - `Account Groups`
2. UI requests API with same `year` and new `scope`.
3. UI renders:
   - `Asset Total`: single series monthly table
   - `Accounts`: multi-series table grouped by account name
   - `Account Groups`: multi-series table grouped by group name

### Flow 4: Read monthly deltas
1. User inspects row for month `M`.
2. UI shows:
   - End balance at end of month `M`
   - Delta versus month `M-1`
   - Delta versus year start baseline
3. Styling:
   - positive: success color
   - negative: danger color
   - zero: muted color

### Flow 5: Empty / no-data scenarios
1. API returns valid payload with empty series.
2. UI shows empty-state card:
   - "No evolution data for selected year/scope."
3. If series exists but all values zero, UI still renders rows (not an error).

## DETAILED PAGE WIREFRAMES

### Reports index card placement

```text
+---------------------------------------------------------------+
| Reports                                                       |
+---------------------------------------------------------------+
| [Monthly Summary] [Category Totals] [Account Totals]         |
| [Account Group Totals] [Asset Total Balance] [Monthly Evol.] |
+---------------------------------------------------------------+
```

### Monthly Evolution main page

```text
+-----------------------------------------------------------------------+
| Monthly Evolution                                      [Back Reports] |
| Year selector: [ 2026 v ]                                             |
| Tabs: [Accounts] [Asset Total] [Account Groups]                       |
+-----------------------------------------------------------------------+
| Status area: [loading spinner] / [error alert]                        |
+-----------------------------------------------------------------------+
| Summary cards (scope-dependent):                                      |
|  - Latest End Balance  - Delta vs Prev (latest)  - Delta vs YTD start |
+-----------------------------------------------------------------------+
| Table (graph-ready data rendered as rows):                            |
|  Series Name | Month | End Balance | Delta Prev | Delta YTD Start     |
|  ------------------------------------------------------------------   |
|  Main Bank   | Jan   | ...         | ...        | ...                 |
|  Main Bank   | Feb   | ...         | ...        | ...                 |
|  ...                                                                  |
+-----------------------------------------------------------------------+
| Placeholder panel: "Chart area reserved for future versions"          |
+-----------------------------------------------------------------------+
```

### Asset Total tab table specialization

```text
+---------------------------------------------------------------+
| Asset Total (single series)                                   |
| Month | End Balance | Delta Prev Month | Delta vs Year Start  |
| Jan   | ...         | ...              | ...                  |
| Feb   | ...         | ...              | ...                  |
| ...                                                        ...|
+---------------------------------------------------------------+
```

## COMPONENT REUSE MATRIX

| Area | Reuse | Modify | New | Notes |
|---|---|---|---|---|
| Reports index navigation | `ReportsIndexPage.razor` | Yes | No | Add Monthly Evolution card |
| Web report visual language | Existing report pages | No | No | Keep card/table/loading/error patterns |
| Money formatting | `MoneyFormatter` | No | No | Continue cents to EUR rendering |
| Date helper | `DateHelper` | Optional minor extension | No | May add current-year month helper if needed |
| API client | `ReportsApi` | Yes | No | Add `GetMonthlyEvolutionAsync` |
| API controller | `ReportsController` | Yes | No | Add new endpoint action |
| Application reporting layer | Existing query/handler pattern | No | Yes | New query + handler + DTOs |
| Infrastructure repository | `ReportingReadRepository` | Yes | No | Add monthly-evolution aggregation path |
| OpenAPI spec | `openspec/api-spec.yaml` | Yes | No | Add endpoint + schemas |
| Tests - API | reporting integration tests folder | Yes | No | Add new test class |
| Tests - Web API client | web tests API folder | Yes | No | Add reports client tests |
| Tests - Web page | web tests features/reports folder | Yes | No | Add page behavior tests |

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: Application DTOs (graph-ready contract)

```csharp
namespace FamilyFinances.Application.Reporting.Dtos;

public enum MonthlyEvolutionScope
{
    Accounts = 1,
    AssetTotal = 2,
    AccountGroups = 3
}

public sealed record MonthlyEvolutionReportDto(
    int Year,
    MonthlyEvolutionScope Scope,
    IReadOnlyList<MonthlyEvolutionSeriesDto> Series
);

public sealed record MonthlyEvolutionSeriesDto(
    string SeriesKey,
    string DisplayName,
    Guid? EntityId,
    string? EntityType,
    IReadOnlyList<MonthlyEvolutionPointDto> Points
);

public sealed record MonthlyEvolutionPointDto(
    int Month,
    DateOnly MonthEndDate,
    long EndBalanceCents,
    long DeltaVsPreviousMonthCents,
    long DeltaVsYearStartCents
);
```

### Example 2: Query + handler contract

```csharp
namespace FamilyFinances.Application.Reporting.Queries;

public sealed record GetMonthlyEvolutionQuery(int Year, MonthlyEvolutionScope Scope);

public sealed class GetMonthlyEvolutionHandler
{
    private readonly IReportingReadRepository _repo;

    public GetMonthlyEvolutionHandler(IReportingReadRepository repo) => _repo = repo;

    public Task<MonthlyEvolutionReportDto> HandleAsync(GetMonthlyEvolutionQuery query, CancellationToken ct)
    {
        if (query.Year < 2000 || query.Year > 2100)
            throw new DomainException("Invalid year.");

        return _repo.GetMonthlyEvolutionAsync(query.Year, query.Scope, ct);
    }
}
```

### Example 3: Repository algorithm skeleton

```csharp
public async Task<MonthlyEvolutionReportDto> GetMonthlyEvolutionAsync(
    int year,
    MonthlyEvolutionScope scope,
    CancellationToken ct)
{
    var monthLimit = year == DateTime.UtcNow.Year ? DateTime.UtcNow.Month : 12;
    var monthEnds = Enumerable.Range(1, monthLimit)
        .Select(m => new DateOnly(year, m, DateTime.DaysInMonth(year, m)))
        .ToArray();

    // 1) Build account-level monthly end balances as canonical base.
    // 2) Derive requested scope from base series.
    // 3) Compute deltas per point.
    // 4) Return ordered graph-ready series payload.
}
```

### Example 4: API endpoint

```csharp
[Authorize(Policy = Policies.CanRead)]
[HttpGet("monthly-evolution")]
public async Task<ActionResult<MonthlyEvolutionReportDto>> GetMonthlyEvolution(
    [FromQuery] int year,
    [FromQuery] MonthlyEvolutionScope scope,
    [FromServices] GetMonthlyEvolutionHandler handler,
    CancellationToken ct)
{
    var dto = await handler.HandleAsync(new GetMonthlyEvolutionQuery(year, scope), ct);
    return Ok(dto);
}
```

### Example 5: Web API client method

```csharp
public async Task<MonthlyEvolutionReportDto> GetMonthlyEvolutionAsync(
    int year,
    MonthlyEvolutionScope scope,
    CancellationToken ct = default)
{
    var token = _tokenStore.GetAccessToken();
    if (string.IsNullOrWhiteSpace(token))
        throw new UnauthorizedAccessException("No access token available.");

    var url = $"api/v1/reports/monthly-evolution?year={year}&scope={scope}";
    using var request = new HttpRequestMessage(HttpMethod.Get, url);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await _http.SendAsync(request, ct);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadFromJsonAsync<MonthlyEvolutionReportDto>(cancellationToken: ct)
        ?? throw new InvalidOperationException("Failed to deserialize monthly evolution response.");
}
```

### Example 6: Razor tab switch and load

```razor
<ul class="nav nav-tabs">
    @foreach (var tab in _tabs)
    {
        <li class="nav-item">
            <button class="nav-link @(tab == _activeScope ? "active" : null)"
                    @onclick="() => ChangeScopeAsync(tab)">
                @tab
            </button>
        </li>
    }
</ul>
```

```csharp
private async Task ChangeScopeAsync(MonthlyEvolutionScope scope)
{
    _activeScope = scope;
    await LoadReportAsync();
}
```

## Risks / Trade-offs

- [Risk] Large payload in Accounts scope for many accounts -> Mitigation: add server-side deterministic ordering and optional future pagination/filtering extension point.
- [Risk] Group totals reflect current membership (no temporal membership history) -> Mitigation: document this behavior explicitly in API spec and report help text.
- [Risk] Snapshot may not exist for all accounts -> Mitigation: use per-account fallback to full-history sum for missing snapshot baseline.
- [Risk] Year filtering semantics confusion for current year -> Mitigation: document and test month window rule (`1..currentMonth`).
- [Risk] Delta bugs from ordering or month gaps -> Mitigation: centralized point builder with deterministic sorted months and unit tests.
- [Risk] Future chart requirements add fields -> Mitigation: include extensible series metadata now (`SeriesKey`, `EntityId`, `EntityType`).

## Migration Plan

1. Add new reporting DTOs and scope enum in `src/FamilyFinances.Application/Reporting/Dtos`.
2. Add query/handler in `src/FamilyFinances.Application/Reporting/Queries` and `src/FamilyFinances.Application/Reporting/Handlers`.
3. Extend `IReportingReadRepository` with monthly evolution method.
4. Implement repository logic in `src/FamilyFinances.Infrastructure/Persistence/Repositories/ReportingReadRepository.cs`.
5. Register handler in `src/FamilyFinances.Infrastructure/DependencyInjection.cs`.
6. Add API action to `src/FamilyFinances.Api/Controllers/V1/ReportsController.cs`.
7. Extend `src/FamilyFinances.Web/Api/ReportsApi.cs`.
8. Add new page `src/FamilyFinances.Web/Components/Pages/Reports/MonthlyEvolutionPage.razor`.
9. Add Reports index card in `src/FamilyFinances.Web/Components/Pages/Reports/ReportsIndexPage.razor`.
10. Add/adjust tests:
   - Application tests for handler and delta semantics
   - API integration tests for endpoint behavior and correctness
   - Web API client tests
   - Web page tests for tab/year/scope rendering and load states
11. Update `openspec/api-spec.yaml`.
12. Run `dotnet build` and `dotnet test`.

Rollback:
- Remove the endpoint, query/handler, DTOs, repository method, web page, and index card.
- Revert OpenAPI additions.
- Keep all existing reports unchanged.

## Open Questions

- Should Accounts scope include all account natures by default, or only balance-oriented natures (Asset/Liability/Equity)?
- Do we want an optional include/exclude closed accounts filter in V1?
- For current year, should the UI offer an override to show all 12 months with placeholders?

## IMPLEMENTATION VERIFICATION CHECKLIST

- [ ] `proposal.md` constraints are reflected in design and implementation.
- [ ] Dedicated route `/reports/monthly-evolution` is used.
- [ ] Reports index includes `Monthly Evolution` card.
- [ ] Existing report pages are not repurposed for this behavior.
- [ ] API endpoint path is `GET /api/v1/reports/monthly-evolution`.
- [ ] Endpoint requires auth policy `CanRead`.
- [ ] Endpoint validates `year` bounds.
- [ ] Endpoint validates `scope` enum values.
- [ ] API response uses cents, not localized currency strings.
- [ ] API response returns stable `SeriesKey`.
- [ ] API response points are sorted by `Month` ascending.
- [ ] `MonthEndDate` values are correct for each month.
- [ ] Current year returns only months `1..currentMonth`.
- [ ] Past year returns months `1..12`.
- [ ] No-activity months are included in output.
- [ ] No-activity month keeps carried end balance.
- [ ] No-activity month delta vs previous is zero.
- [ ] Delta vs previous month formula is correct for all months.
- [ ] Delta vs year start formula is correct for all months.
- [ ] January delta vs year start uses previous-year close baseline.
- [ ] Repository method is added to `IReportingReadRepository`.
- [ ] Repository implementation remains in Infrastructure only.
- [ ] Application handler contains only orchestration/validation.
- [ ] Controller does not perform reporting calculations directly.
- [ ] Asset Total scope includes only `AccountNature.Asset`.
- [ ] Asset Total scope produces a single series.
- [ ] Accounts scope series contain account identifiers.
- [ ] Account Groups scope series contain group identifiers.
- [ ] Group aggregation uses member account series consistently.
- [ ] Series display names are non-empty.
- [ ] Empty dataset returns `200 OK` with empty series.
- [ ] Unauthorized request returns `401/403` consistent with existing reports.
- [ ] `ReportsApi` method handles missing token consistently.
- [ ] `ReportsApi` method throws on unauthorized consistently.
- [ ] Web page has year selector.
- [ ] Web page has three scope tabs.
- [ ] Web page has loading state.
- [ ] Web page has error state.
- [ ] Web page has empty state.
- [ ] Web page renders data table for current scope.
- [ ] Web page uses `MoneyFormatter` for display amounts.
- [ ] Positive/negative/zero colors follow existing style rules.
- [ ] UI does not import chart libraries in this change.
- [ ] UI includes explicit chart placeholder area.
- [ ] API integration tests cover inclusive month behavior.
- [ ] API integration tests cover no-data behavior.
- [ ] API integration tests cover Accounts scope.
- [ ] API integration tests cover Asset Total scope.
- [ ] API integration tests cover Account Groups scope.
- [ ] Application tests cover delta computations.
- [ ] Web API tests cover request URL shape and parsing.
- [ ] Web page tests cover tab switching and reload behavior.
- [ ] OpenAPI includes endpoint definition and parameters.
- [ ] OpenAPI includes response schemas for report/series/point.
- [ ] `dotnet build` succeeds with zero warnings.
- [ ] `dotnet test` succeeds and remains deterministic.
