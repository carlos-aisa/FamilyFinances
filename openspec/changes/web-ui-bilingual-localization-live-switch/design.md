## Context

The Web application currently renders with mixed localization patterns:

- `src/FamilyFinances.Web/Components/App.razor` declares `<html lang="en">` statically.
- Several pages force `es-ES` explicitly for date/currency rendering.
- Other pages rely on `CultureInfo.CurrentCulture`.
- Report/format helper utilities use `CultureInfo.InvariantCulture`, producing non-user-locale output.
- There is no localization middleware/configuration in `src/FamilyFinances.Web/Program.cs`.
- There is no language selector in the shared shell, and users cannot switch language at runtime.

Stakeholders:

- Primary: End users that want Spanish or English UI without relogging or browser-level language hacks.
- Secondary: Maintainers who need a predictable localization approach instead of page-level hardcoding.

Constraints:

- Scope is the Blazor Web UI only.
- Backend/API/domain error payload localization is out of scope.
- Architecture must remain within existing Blazor Interactive Server + API client boundaries.
- Changes must not alter business behavior or API contracts.

## IMPLEMENTATION RULES - DO NOT DEVIATE

- Do not add any new frontend framework or state management framework.
- Do not bypass existing shell composition (`App.razor` + `Routes.razor` + `MainLayout` + `NavMenu`).
- Do not hardcode language strings in newly touched pages; use resources.
- Do not introduce additional supported cultures beyond `es-ES` and `en-US` in this change.
- Do not localize API/server exception payloads in this change.
- Do not change endpoint URLs, DTO contracts, or authorization policies.
- Keep current route structure unchanged.
- All language switching must be immediate via current-route reload.
- Persist language preference client-side and reapply it on next visit.
- Ensure formatting uses active UI culture for dates and currency in touched components.

## Goals / Non-Goals

**Goals:**

- Provide runtime UI language selector in shared navigation.
- Support `es-ES` and `en-US` consistently.
- Apply language change immediately in the current screen.
- Persist chosen language so refreshed/new sessions reuse it.
- Replace hardcoded/Invariant formatting patterns in targeted pages/helpers with culture-driven formatting.
- Add/adjust Web tests validating language-dependent formatting behavior.

**Non-Goals:**

- Backend/API localization.
- Adding additional languages.
- Redesigning layout/navigation structure.
- Introducing tenant/user-profile server-side language storage.

## Decisions

### Decision 1: Use ASP.NET Core localization middleware in Web project only

- Decision: Configure `AddLocalization()` and `UseRequestLocalization()` in `src/FamilyFinances.Web/Program.cs` with supported cultures `es-ES` and `en-US`.
- Rationale: Standard .NET localization flow integrates naturally with Blazor server rendering and `CultureInfo.CurrentCulture`.
- Alternative considered: Keep ad-hoc formatting + manual dictionary in C# classes.
- Rejected because: It does not scale and keeps today’s inconsistency.

### Decision 2: Use CookieRequestCultureProvider + localStorage to persist and apply language

- Decision: Set `.AspNetCore.Culture` cookie plus a localStorage mirror key from client JS helper; language switch triggers `NavigationManager.NavigateTo(currentUri, forceLoad: true)`.
- Rationale: Cookie is understood by request localization middleware on full reload; localStorage preserves explicit user preference on client side.
- Alternative considered: Query-string-only culture handling.
- Rejected because: It pollutes URLs and complicates internal navigation.

### Decision 3: Place language selector in existing NavMenu top row

- Decision: Extend `src/FamilyFinances.Web/Components/Layout/NavMenu.razor` top controls (theme toggle zone) with language dropdown.
- Rationale: Selector is globally available in one consistent place, no per-page duplication.
- Alternative considered: Dedicated settings page.
- Rejected because: Requires extra navigation for a high-frequency preference.

### Decision 4: Localize UI strings via resource files and localizers in touched components

- Decision: Add shared resource model (for shell/common strings) and page/component resources for high-traffic screens (transactions, accounts movements, reports index, date presets, dashboard quick-entry labels already in scope).
- Rationale: Resource-driven text is maintainable and testable.
- Alternative considered: Keep only formatting localized and leave labels in English.
- Rejected because: User requirement includes selectable UI language.

### Decision 5: Standardize formatting paths on active culture

- Decision: Remove forced culture usage in targeted UI rendering paths and use `CurrentCulture`/`CurrentUICulture` semantics through framework formatting.
- Rationale: Selected language should drive visible date/month/currency outputs.
- Alternative considered: Continue hardcoded `es-ES` for date while localizing text only.
- Rejected because: Mixed-language UX remains broken.

### Decision 6: Keep API and persistence layers unchanged

- Decision: Limit modifications to `src/FamilyFinances.Web` and `tests/FamilyFinances.Web.Tests`.
- Rationale: Requirement explicitly scopes localization to Web UI.
- Alternative considered: Localize domain/API errors.
- Rejected because: out of scope and higher regression surface.

## DETAILED UI FLOWS AND COMPONENT REUSE

### Flow 1: First load with no saved culture

1. User opens any Web route.
2. `UseRequestLocalization()` resolves culture using provider chain; no culture cookie found.
3. Default culture `es-ES` is applied.
4. `NavMenu` renders language selector showing Spanish selected.
5. All touched pages render labels and formatting in Spanish.

Component reuse:

- Reuse existing `NavMenu` top row controls; append selector beside theme toggle.
- Reuse existing page components; replace text bindings and formatting paths only.

### Flow 2: User switches language from Spanish to English

1. User clicks language selector in `NavMenu`.
2. Selector invokes JS helper to set culture cookie + localStorage value (`en-US`).
3. `NavMenu` calls `NavigationManager.NavigateTo(currentUri, forceLoad: true)`.
4. Browser reloads same URL.
5. Middleware reads cookie and applies `en-US`.
6. Same page re-renders immediately in English.

Component reuse:

- Reuse existing `theme.js` style integration pattern (`window.*Helper`) with a dedicated `culture.js` helper.

### Flow 3: User returns later

1. User returns to app in a new browser session.
2. Culture cookie remains; middleware sets previous language.
3. Shell and pages render directly in stored language without manual action.

### Flow 4: Transaction list rendering under current culture

1. User navigates to `/transactions`.
2. Page reads transaction DTOs unchanged.
3. Date text and amount format use active culture.
4. Labels and action strings are read from resources.

### Flow 5: Account movements rendering under current culture

1. User navigates to `/accounts/{id}/movements`.
2. Table headers, filter labels, empty states use resources.
3. Movement date and running balance/amount formatting use active culture.

### Flow 6: Missing/invalid saved culture fallback

1. User has stale/unsupported culture value in localStorage.
2. JS helper normalizes value to supported set.
3. If unsupported, it writes default `es-ES` and reloads once.
4. App renders with default language safely.

## DETAILED PAGE WIREFRAMES

### Main shell top controls

```text
+-------------------------------------------------------------+
| FamilyFinances                               [Theme] [Lang?]|
+-------------------------------------------------------------+
| Nav items...                                                 |
| - Home                                                       |
| - Accounts                                                   |
| - Account Groups                                             |
| - Payees                                                     |
| - Transactions                                               |
| - Reports                                                    |
+-------------------------------------------------------------+
```

### Language selector menu

```text
+---------------------+
| Language            |
| ------------------- |
| (*) Espanol         |
| ( ) English         |
+---------------------+
```

### Transactions list header (localized)

```text
+------------------------------------------------------------------+
| [Transactions]                                [Refresh] [New ?]   |
| "N transactions" (localized pluralization strategy)             |
+------------------------------------------------------------------+
| Filters: [Quick Date Filter] [From Date] [To Date] [Search]      |
+------------------------------------------------------------------+
| Date | Type | Description | Amount                                |
+------------------------------------------------------------------+
```

## COMPONENT REUSE MATRIX

| Area | Existing Component/File | Action | Notes |
|---|---|---|---|
| App shell language metadata | `src/FamilyFinances.Web/Components/App.razor` | Modify | Bind `lang` behavior to selected culture rather than static `en` |
| Global nav controls | `src/FamilyFinances.Web/Components/Layout/NavMenu.razor` | Modify | Add language selector UI + event handling |
| Theme helper pattern | `src/FamilyFinances.Web/wwwroot/js/theme.js` | Reuse pattern | Create analogous culture helper script |
| Transactions list page | `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionsListPage.razor` | Modify | Localize labels and culture-based formatting |
| Transaction detail page | `src/FamilyFinances.Web/Components/Pages/Transactions/TransactionDetailPage.razor` | Modify | Localize labels; remove hardcoded `es-ES` date formatting |
| Account movements page | `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor` | Modify | Localize labels; remove hardcoded date culture |
| Date presets shared component | `src/FamilyFinances.Web/Components/Shared/DateRangePresets.razor` | Modify | Localize preset labels |
| Date helper | `src/FamilyFinances.Web/Features/Reports/DateHelper.cs` | Modify | Avoid invariant month names for UI text paths |
| Money formatter | `src/FamilyFinances.Web/Features/Reports/MoneyFormatter.cs` | Modify | Use active culture-aware formatting for localized outputs |
| Resource infrastructure | `src/FamilyFinances.Web/Resources/**` (new) | New | Add localization resource files for `es-ES` and `en-US` |
| Web tests | `tests/FamilyFinances.Web.Tests/**` | Modify/Add | Add culture-dependent formatting and selector behavior tests |

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: Web localization registration

```csharp
// src/FamilyFinances.Web/Program.cs
using Microsoft.AspNetCore.Localization;
using System.Globalization;

builder.Services.AddLocalization();

var supportedCultures = new[]
{
    new CultureInfo("es-ES"),
    new CultureInfo("en-US")
};

var app = builder.Build();

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-ES"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(localizationOptions);
```

### Example 2: Culture JS helper with cookie + localStorage

```javascript
// src/FamilyFinances.Web/wwwroot/js/culture.js
(function () {
  const STORAGE_KEY = "ff_culture";
  const COOKIE_KEY = ".AspNetCore.Culture";
  const SUPPORTED = ["es-ES", "en-US"];

  function normalize(culture) {
    return SUPPORTED.includes(culture) ? culture : "es-ES";
  }

  function setCulture(culture) {
    const c = normalize(culture);
    localStorage.setItem(STORAGE_KEY, c);
    document.cookie = `${COOKIE_KEY}=c=${c}|uic=${c};path=/;max-age=31536000`;
    return c;
  }

  function getCulture() {
    return normalize(localStorage.getItem(STORAGE_KEY) || "es-ES");
  }

  window.cultureHelper = { getCulture, setCulture };
})();
```

### Example 3: NavMenu selector behavior

```razor
<select class="form-select form-select-sm"
        value="@_culture"
        @onchange="OnCultureChanged">
    <option value="es-ES">Espanol</option>
    <option value="en-US">English</option>
</select>

@code {
    private string _culture = "es-ES";

    private async Task OnCultureChanged(ChangeEventArgs args)
    {
        var selected = args.Value?.ToString() ?? "es-ES";
        _culture = await JS.InvokeAsync<string>("cultureHelper.setCulture", selected);
        Nav.NavigateTo(Nav.Uri, forceLoad: true);
    }
}
```

### Example 4: Localized string usage in a page

```razor
@inject Microsoft.Extensions.Localization.IStringLocalizer<TransactionsListPage> L

<h3 class="mb-1">@L["Transactions_Title"]</h3>
<p class="text-muted mb-0">@L["Transactions_Subtitle"]</p>
```

### Example 5: Culture-driven date/currency formatting

```csharp
private static string FormatAmount(decimal amount)
    => amount.ToString("C2", CultureInfo.CurrentCulture);

private static string FormatShortDate(DateOnly date)
    => date.ToString("dd MMM", CultureInfo.CurrentCulture);
```

## CRITICAL UX BEHAVIORS

- Language switch must be immediate and visible in current screen after selector change.
- Selected language must persist across refresh and browser restart.
- Selector must always show active language accurately.
- Date and currency output must align with selected language/culture.
- Existing business information hierarchy, button placement, and navigation flow must not change.
- No loss of existing theme toggle behavior in nav bar.

## Risks / Trade-offs

- [Risk] Resource key sprawl across many pages can create inconsistent naming.
  -> Mitigation: Use a deterministic key naming convention per component (`<Component>_<Section>_<Name>`).

- [Risk] Partial localization (some untouched pages) can produce mixed-language UI during rollout.
  -> Mitigation: Prioritize all main navigation destinations in this change and track residual keys in tasks checklist.

- [Risk] Force reload after language change interrupts unsaved forms.
  -> Mitigation: Keep selector in shell and document current behavior; avoid auto-switch without explicit user action.

- [Risk] Culture cookie and localStorage can diverge.
  -> Mitigation: `cultureHelper.setCulture()` writes both in a single path.

- [Trade-off] Full route reload is less seamless than component-only rerender.
  -> Mitigation: It guarantees middleware culture consistency in Interactive Server rendering.

## Migration Plan

1. Add localization service and middleware in Web startup.
2. Add `culture.js` helper and include script in `App.razor`.
3. Add language selector UI in `NavMenu` and hook to helper + force reload.
4. Introduce resource files for shell/shared/components in scope.
5. Replace hardcoded UI strings and hardcoded culture formatting in targeted pages/helpers.
6. Add/adjust Web tests for localized formatting and selector behavior.
7. Build and run web test project.
8. Validate manual acceptance for both languages on key pages.

Rollback strategy:

- Remove localization registration and selector wiring.
- Remove new resource files and restore static strings/formatting behavior.
- Keep API and DB untouched.

## Open Questions

- No blocking open questions for implementation scope.

## IMPLEMENTATION VERIFICATION CHECKLIST

### A) Startup and infrastructure

- ? `AddLocalization()` configured in Web startup.
- ? `UseRequestLocalization()` enabled before component rendering.
- ? Supported cultures include exactly `es-ES` and `en-US`.
- ? Default culture set to `es-ES`.
- ? No localization service registration added to API project.
- ? No backend DTO/API contract changes introduced.

### B) Shell and selector behavior

- ? Selector is placed in `NavMenu` top controls, not per-page.
- ? Selector displays both languages with clear labels.
- ? Selector initial value reflects active culture.
- ? Changing selector updates culture cookie.
- ? Changing selector updates localStorage key.
- ? Changing selector reloads current route with `forceLoad: true`.
- ? Theme toggle still works after language selector integration.
- ? No visual overlap/regression in desktop sidebar top row.
- ? No visual overlap/regression in collapsed/mobile nav.
- ? `App.razor` language metadata reflects active culture strategy.

### C) Resource coverage

- ? Resource files exist for both `es-ES` and `en-US` for touched components.
- ? Shared layout strings moved out of hardcoded text.
- ? Transactions list strings localized.
- ? Transaction detail strings localized.
- ? Account movements strings localized.
- ? Date preset labels localized.
- ? Reports index key titles/descriptions localized.
- ? Dashboard quick-entry visible labels in scope localized.
- ? No new hardcoded user-facing text introduced in touched files.
- ? Resource key naming convention is consistent.

### D) Formatting behavior

- ? Hardcoded `new CultureInfo("es-ES")` removed from touched formatting outputs.
- ? `InvariantCulture` no longer drives user-facing localized month/date/currency text in touched helpers.
- ? Currency uses active culture separators and symbol order.
- ? Month names follow active language.
- ? Day names in transaction detail follow active language.
- ? Running balance formatting follows active culture.
- ? Transactions amount badges keep existing semantic colors.
- ? Numeric sign behavior remains unchanged.

### E) Functional regression guards

- ? Navigation routes remain unchanged.
- ? Existing auth gates (`AuthorizeView`) remain unchanged.
- ? Filter/query behaviors remain unchanged.
- ? Account movement loading behavior remains unchanged.
- ? Transaction create/edit/delete business behavior remains unchanged.
- ? Reports API calls remain unchanged.

### F) Testing

- ? Web test project compiles.
- ? Existing relevant tests updated for localization-aware formatting where needed.
- ? New tests cover month/date/currency culture variation.
- ? New tests cover formatter behavior for both cultures.
- ? New tests avoid environment-dependent flakiness (set culture explicitly in test scope).
- ? No EF integration test provider changes introduced.

### G) Documentation and quality

- ? All added documentation/comments are in English.
- ? Proposal/design/spec/tasks stay consistent with final scope.
- ? Rollback steps are documented and feasible.
- ? No architecture boundary violations introduced.
- ? No new third-party dependency added without justification.
- ? Final manual verification includes both language paths on key routes.
