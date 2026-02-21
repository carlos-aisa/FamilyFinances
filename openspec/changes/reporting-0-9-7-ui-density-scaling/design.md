## Context

The current Web UI uses a single effective density profile (Bootstrap defaults plus local overrides), which becomes hard to use on small or low-resolution devices and reduces table readability in dense report pages. Theme preferences are already stored in browser local storage via `src/FamilyFinances.Web/wwwroot/js/theme.js`, but there is no equivalent mechanism for app-wide density selection and no centralized settings surface.

This change must stay within current architecture constraints:
- no backend persistence for UI preference state
- no API contract changes
- Blazor Interactive Server with JS interop for browser-only state
- additive navigation and styling changes without breaking existing routes

Stakeholders:
- end users who need readable/compact views across laptop and desktop resolutions
- reporting users who work with wide tables and KPI cards
- maintainers who need one place to evolve preference settings (theme, language, backup/restore in future releases)

## Goals / Non-Goals

**Goals:**
- Deliver app-wide density scaling with exactly four levels: `Small`, `Medium`, `Large`, `XLarge`.
- Persist user density preference in browser local storage and restore it on startup.
- Apply automatic compact density for constrained viewports when the user has not explicitly selected a size.
- Introduce a dedicated settings page where theme and density are managed in one place.
- Keep future extension points visible for language and backup/restore without implementing them in this change.
- Keep current business logic and reporting API behavior unchanged.

**Non-Goals:**
- Server-side storage/sync of preferences.
- New API endpoints or database schema changes.
- Rewriting report pages for custom component libraries.
- Full i18n implementation or backup/restore workflows.

## IMPLEMENTATION RULES - DO NOT DEVIATE

- [MUST] Use global CSS custom properties and a single HTML data attribute (`data-ui-scale`) as the density source of truth.
- [MUST] Keep density state client-side only (`localStorage`).
- [MUST] Keep setting changes live (no page reload required for user-triggered updates).
- [MUST] Support explicit user override over auto-density behavior.
- [MUST] Keep the existing theme storage key (`ff_theme`) compatible.
- [MUST NOT] Introduce page-specific inline size tweaks as the primary mechanism.
- [MUST NOT] Couple density logic to report data loading or API calls.
- [MUST NOT] add backend configuration endpoints for appearance preferences.

## DETAILED UI FLOWS AND COMPONENT REUSE

### Flow 1: First visit on constrained viewport (auto mode)
1. User opens app with no stored density key.
2. `ui-preferences.js` evaluates viewport constraints (`matchMedia` + width/height threshold).
3. Script sets `document.documentElement.dataset.uiScale = "small"` and `data-ui-scale-source = "auto"`.
4. CSS tokens resolve to compact typography/control sizes.
5. Settings page (when opened later) displays selected level as `Small` with origin `Auto`.

Reuse:
- Reuse existing startup script loading pattern from `theme.js`.
- Reuse root-level attribute pattern currently used by `data-bs-theme`.

### Flow 2: User selects explicit density from Settings
1. User navigates to `/settings`.
2. User chooses `Medium`, `Large`, or `XLarge` from density segmented control/select.
3. Settings page calls JS helper `setUiScale(level, "user")`.
4. JS helper writes `ff_ui_scale` and `ff_ui_scale_source=user` in local storage.
5. Root HTML `data-ui-scale` updates immediately; all pages/components adjust without reload.

Reuse:
- Reuse Blazor event handling patterns used in existing forms (`@onchange` + local state).
- Reuse JS interop approach already used by `NavMenu` theme toggle.

### Flow 3: User resets to automatic behavior
1. User clicks `Use automatic size` in `/settings`.
2. Settings page calls JS helper `clearUiScalePreference()`.
3. Helper removes explicit keys, recomputes density from viewport, sets source `auto`.
4. UI updates instantly; settings label reflects `Auto`.

Reuse:
- Reuse local storage access helper from same JS module.
- Reuse visual button style from existing bootstrap controls.

### Flow 4: Theme management from Settings
1. User opens `/settings` and toggles dark/light mode.
2. Page calls existing `themeHelper.setTheme(...)`.
3. `data-bs-theme` updates as currently implemented.
4. If quick toggle remains in navigation, both entry points stay consistent because both target the same storage key.

Reuse:
- Reuse `theme.js` without breaking existing public API (`getTheme`, `setTheme`, `toggleTheme`).

### Flow 5: Authenticated navigation to Settings
1. Authenticated user opens sidebar menu.
2. New `Settings` nav link appears under existing sections.
3. Selecting link routes to `/settings`.
4. Unauthenticated users do not get settings entry due to existing `AuthorizeView` pattern.

Reuse:
- Reuse nav link structure already used in `src/FamilyFinances.Web/Components/Layout/NavMenu.razor`.

## DETAILED PAGE WIREFRAMES

### Sidebar navigation section

```text
+--------------------------------------------------+
| FamilyFinances                                   |
| [theme quick action optional]                    |
+--------------------------------------------------+
| Home                                             |
| Accounts                                         |
| Account Groups                                   |
| Payees                                           |
| Transactions                                     |
| History                                          |
| Reports                                          |
| Settings  <-- NEW                               |
| Logout                                           |
+--------------------------------------------------+
```

### Settings page layout (`/settings`)

```text
+----------------------------------------------------------------------------------+
| Settings                                                                         |
| Manage appearance and future user preferences                                    |
+----------------------------------------------------------------------------------+
| Appearance                                                                       |
|  Theme:     ( ) Dark   ( ) Light                                                 |
|  Density:   [ Small | Medium | Large | XLarge ]                                  |
|  Status:    Applied by: User/Auto                                                |
|  Actions:   [Use automatic size]                                                 |
+----------------------------------------------------------------------------------+
| Language (Planned)                                                               |
|  Placeholder copy: "Will be enabled in next 0.9.x change."                       |
+----------------------------------------------------------------------------------+
| Backup/Restore (Planned)                                                         |
|  Placeholder copy: "Will be enabled in next 0.9.x change."                       |
+----------------------------------------------------------------------------------+
```

### Visual density behavior for report tables

```text
Small:  reduced row height + tighter card padding + smaller headings
Medium: baseline current visual size (default for unconstrained desktop)
Large:  increased text + controls + spacing
XLarge: maximum readable mode (no layout break guarantees beyond responsive rules)
```

## COMPONENT REUSE MATRIX

| Area | Reuse | Modify | New |
|---|---|---|---|
| Theme storage and HTML theme attribute | `wwwroot/js/theme.js` functions | Extend usage from settings page | None |
| Main app startup scripts | `Components/App.razor` script include pattern | Add include for density helper script | None |
| Sidebar navigation | `Components/Layout/NavMenu.razor` auth/nav sections | Add `Settings` entry and optional quick access behavior | None |
| Shared app styling | `wwwroot/app.css` and existing bootstrap token usage | Add density tokens and selectors | `wwwroot/ui-density.css` (or equivalent dedicated density block) |
| Settings UI | Existing page composition patterns (cards/forms) | N/A | `Components/Pages/Settings/SettingsPage.razor` (+ optional `.razor.css`) |
| Client preference orchestration | Existing direct JS interop in components | Introduce reusable wrapper service | `State/IUiPreferencesService.cs`, `State/UiPreferencesService.cs` |
| Web tests | bUnit test setup patterns | Add settings and nav tests, density state assertions | new tests under `tests/FamilyFinances.Web.Tests/Features/Settings` |

## Decisions

### Decision 1: Represent density with root-level data attributes
- **Choice:** Use `data-ui-scale` and `data-ui-scale-source` on `<html>`.
- **Rationale:** Mirrors current theme mechanism, keeps CSS resolution centralized, and avoids per-component state propagation.
- **Alternative considered:** Cascading Blazor parameter with per-component class composition.
  - **Rejected because:** high implementation overhead and inconsistent coverage risk across existing pages.

### Decision 2: Keep current visual baseline as `Medium`
- **Choice:** Map current effective sizing to `Medium`; use `Small` for compact auto mode.
- **Rationale:** Minimizes regression risk and user surprise for existing desktop users.
- **Alternative considered:** Treat current baseline as `Large`.
  - **Rejected because:** would force broad downscaling for most users and increase perceived regression risk.

### Decision 3: Auto mode applies only when no explicit user selection exists
- **Choice:** Run viewport-based auto selection only when `ff_ui_scale` is absent.
- **Rationale:** User intent must override heuristics to avoid preference churn.
- **Alternative considered:** Auto-recompute on every resize even with explicit selection.
  - **Rejected because:** violates explicit preference expectation.

### Decision 4: Settings page is canonical preference surface
- **Choice:** Add `/settings` as explicit preference page and keep quick theme control optional.
- **Rationale:** Aligns with requested roadmap (theme, language, density, backup/restore) and reduces future navigation churn.
- **Alternative considered:** Keep only sidebar quick toggles.
  - **Rejected because:** does not scale for future preference categories.

### Decision 5: Introduce typed UI preference service for JS interop
- **Choice:** Wrap JS calls in `IUiPreferencesService`.
- **Rationale:** avoids duplicate string-based JS calls across components and improves testability.
- **Alternative considered:** keep direct `IJSRuntime.InvokeAsync` calls in each component.
  - **Rejected because:** brittle and duplicated interop signatures.

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: JS helper surface (`src/FamilyFinances.Web/wwwroot/js/ui-preferences.js`)

```javascript
(function () {
  const SCALE_KEY = "ff_ui_scale";
  const SCALE_SOURCE_KEY = "ff_ui_scale_source";
  const allowed = ["small", "medium", "large", "xlarge"];

  function resolveAutoScale() {
    const isMobile = window.matchMedia("(max-width: 991.98px)").matches;
    const isLowRes = window.innerWidth <= 1366 || window.innerHeight <= 768;
    return (isMobile || isLowRes) ? "small" : "medium";
  }

  function applyScale(scale, source) {
    document.documentElement.setAttribute("data-ui-scale", scale);
    document.documentElement.setAttribute("data-ui-scale-source", source);
  }

  function setScale(scale, source) {
    const normalized = (scale || "").toLowerCase();
    if (!allowed.includes(normalized)) return getScale();
    if (source === "user") {
      localStorage.setItem(SCALE_KEY, normalized);
      localStorage.setItem(SCALE_SOURCE_KEY, "user");
    }
    applyScale(normalized, source ?? "user");
    return normalized;
  }

  function getScale() {
    return localStorage.getItem(SCALE_KEY) || resolveAutoScale();
  }

  function init() {
    const stored = localStorage.getItem(SCALE_KEY);
    if (stored && allowed.includes(stored)) {
      applyScale(stored, "user");
      return;
    }
    applyScale(resolveAutoScale(), "auto");
  }

  window.uiPreferencesHelper = {
    init,
    getScale,
    setScale: (scale) => setScale(scale, "user"),
    clearScalePreference: () => {
      localStorage.removeItem(SCALE_KEY);
      localStorage.removeItem(SCALE_SOURCE_KEY);
      applyScale(resolveAutoScale(), "auto");
      return getScale();
    },
    getScaleSource: () => localStorage.getItem(SCALE_SOURCE_KEY) || "auto"
  };

  init();
})();
```

### Example 2: Root token mapping (`src/FamilyFinances.Web/wwwroot/ui-density.css`)

```css
:root {
  --ff-font-size-base: 1rem;
  --ff-line-height-base: 1.5;
  --ff-control-min-height: 2.5rem;
  --ff-card-padding-y: 1rem;
  --ff-card-padding-x: 1rem;
  --ff-table-cell-padding-y: 0.75rem;
  --ff-table-cell-padding-x: 0.75rem;
}

html[data-ui-scale="small"] {
  --ff-font-size-base: 0.9rem;
  --ff-line-height-base: 1.35;
  --ff-control-min-height: 2.1rem;
  --ff-card-padding-y: 0.65rem;
  --ff-card-padding-x: 0.75rem;
  --ff-table-cell-padding-y: 0.45rem;
  --ff-table-cell-padding-x: 0.55rem;
}

html[data-ui-scale="large"] {
  --ff-font-size-base: 1.08rem;
  --ff-control-min-height: 2.8rem;
}

html[data-ui-scale="xlarge"] {
  --ff-font-size-base: 1.18rem;
  --ff-control-min-height: 3.1rem;
}
```

### Example 3: Token consumption in shared styles (`src/FamilyFinances.Web/wwwroot/app.css`)

```css
body {
  font-size: var(--ff-font-size-base);
  line-height: var(--ff-line-height-base);
}

.form-control,
.form-select,
.btn {
  min-height: var(--ff-control-min-height);
}

.card-body {
  padding: var(--ff-card-padding-y) var(--ff-card-padding-x);
}

.table > :not(caption) > * > * {
  padding: var(--ff-table-cell-padding-y) var(--ff-table-cell-padding-x);
}
```

### Example 4: Settings page density binding (`src/FamilyFinances.Web/Components/Pages/Settings/SettingsPage.razor`)

```razor
@page "/settings"
@inject IUiPreferencesService UiPreferences

<h1>Settings</h1>

<div class="card mb-3">
  <div class="card-header">Appearance</div>
  <div class="card-body">
    <label class="form-label">Density</label>
    <select class="form-select" value="@_selectedScale" @onchange="OnScaleChanged">
      <option value="small">Small</option>
      <option value="medium">Medium</option>
      <option value="large">Large</option>
      <option value="xlarge">XLarge</option>
    </select>
  </div>
</div>
```

### Example 5: Typed interop service (`src/FamilyFinances.Web/State/UiPreferencesService.cs`)

```csharp
using Microsoft.JSInterop;

namespace FamilyFinances.Web.State;

public sealed class UiPreferencesService : IUiPreferencesService
{
    private readonly IJSRuntime _js;

    public UiPreferencesService(IJSRuntime js) => _js = js;

    public ValueTask<string> GetScaleAsync()
        => _js.InvokeAsync<string>("uiPreferencesHelper.getScale");

    public ValueTask<string> SetScaleAsync(string scale)
        => _js.InvokeAsync<string>("uiPreferencesHelper.setScale", scale);

    public ValueTask<string> ClearScalePreferenceAsync()
        => _js.InvokeAsync<string>("uiPreferencesHelper.clearScalePreference");
}
```

## CRITICAL UX BEHAVIORS

- Density selection updates visible UI immediately in the same session.
- Automatic compact mode is deterministic and stable until explicit user override.
- Settings page must show source context (`Auto` vs `User`) for transparency.
- Theme and density controls are independent; changing one must not reset the other.
- `XLarge` remains available even if some dense tables require horizontal scroll.
- Report pages should preserve numeric alignment and sign coloring regardless of density mode.
- Sidebar link and settings page must remain accessible in both dark and light themes.
- If JS interop fails, app falls back to `medium` tokens and remains usable.

## Risks / Trade-offs

- [Risk] Density token coverage misses isolated hardcoded sizes -> visual inconsistency.
  - Mitigation: run CSS grep for fixed `font-size`, `height`, and table paddings in `Components/**/*.razor.css` and `wwwroot/app.css`.
- [Risk] `Small` mode can reduce readability for some users.
  - Mitigation: settings page always allows immediate switch to larger levels.
- [Risk] `XLarge` can increase scroll in data-heavy pages.
  - Mitigation: preserve responsive wrappers and table overflow handling.
- [Risk] JS load order issues can cause first-render flicker.
  - Mitigation: include `ui-preferences.js` in `<head>` near `theme.js` and run `init()` immediately.
- [Trade-off] Additional client-side complexity in JS helpers.
  - Mitigation: keep helper API small and typed via service wrapper.

## Migration Plan

1. Add UI preference JS helper and load script in `Components/App.razor`.
2. Add density token stylesheet and token consumption in shared app CSS.
3. Add typed `IUiPreferencesService` and register service in `Program.cs`.
4. Create `/settings` page with Theme + Density controls and placeholders for future options.
5. Add `Settings` nav entry for authenticated users.
6. Update/align existing theme quick toggle behavior to stay consistent with settings page.
7. Add Web tests for navigation, settings rendering, density options, and auto/user source visibility.
8. Run Web test suite and reporting smoke tests.

### Rollback Strategy

1. Remove settings route link from navigation if severe regressions appear.
2. Disable `ui-preferences.js` include and force `data-ui-scale="medium"` in markup/CSS default.
3. Keep theme behavior unchanged by preserving `theme.js`.
4. Re-run web tests and confirm baseline UI behavior.

## Open Questions

- Should quick theme toggle remain in sidebar after settings page is introduced, or move fully into settings in `0.9.8`?
- Should low-resolution threshold be fixed (`1366x768`) or configurable via app settings file for deployment-specific tuning?
- Should `XLarge` apply the same table density scaling as cards/forms, or use a capped table multiplier for wide report matrices?

## IMPLEMENTATION VERIFICATION CHECKLIST

### Architecture and setup
- [ ] Confirm `src/FamilyFinances.Web/Program.cs` registers `IUiPreferencesService`.
- [ ] Confirm no backend API or database code changed for this feature.
- [ ] Confirm settings feature lives entirely in Web layer.
- [ ] Confirm no cross-layer dependency violations were introduced.
- [ ] Confirm script and style assets are loaded in `Components/App.razor`.
- [ ] Confirm asset load order keeps preference init before first render interaction.

### JS helper behavior
- [ ] Confirm `uiPreferencesHelper` exists on `window`.
- [ ] Confirm allowed scale values are exactly `small`, `medium`, `large`, `xlarge`.
- [ ] Confirm invalid input does not break state and returns current value.
- [ ] Confirm `setScale` writes `ff_ui_scale` in local storage.
- [ ] Confirm `clearScalePreference` removes stored keys.
- [ ] Confirm `getScaleSource` returns `user` after explicit selection.
- [ ] Confirm `getScaleSource` returns `auto` when no explicit selection exists.
- [ ] Confirm auto scale resolves to `small` on mobile width.
- [ ] Confirm auto scale resolves to `small` on low-height viewport.
- [ ] Confirm auto scale resolves to `medium` on unconstrained desktop.

### CSS token coverage
- [ ] Confirm root token defaults map to `medium`.
- [ ] Confirm `small` profile changes font size and control height.
- [ ] Confirm `large` profile increases baseline sizes.
- [ ] Confirm `xlarge` profile increases baseline sizes beyond `large`.
- [ ] Confirm body typography consumes token values.
- [ ] Confirm button/form control heights consume token values.
- [ ] Confirm card paddings consume token values.
- [ ] Confirm table cell paddings consume token values.
- [ ] Confirm report page headers still wrap correctly in `small`.
- [ ] Confirm table overflow is still manageable in `xlarge`.

### Settings page UX
- [ ] Confirm `/settings` route resolves for authenticated users.
- [ ] Confirm page title and description are visible.
- [ ] Confirm appearance section includes theme control.
- [ ] Confirm appearance section includes density control.
- [ ] Confirm density options display all four levels.
- [ ] Confirm selected density reflects current state on load.
- [ ] Confirm source label (Auto/User) is visible and accurate.
- [ ] Confirm "Use automatic size" action is available.
- [ ] Confirm placeholder sections for language and backup/restore are visible.
- [ ] Confirm settings page is usable in dark mode.
- [ ] Confirm settings page is usable in light mode.

### Navigation behavior
- [ ] Confirm `Settings` nav link appears for authenticated users.
- [ ] Confirm `Settings` nav link is hidden for unauthenticated users.
- [ ] Confirm navigation to `/settings` works from sidebar.
- [ ] Confirm navigation highlight/active state works for `/settings`.
- [ ] Confirm adding settings entry does not break existing nav items.

### Persistence and startup behavior
- [ ] Confirm explicit user density persists across page reloads.
- [ ] Confirm explicit user density persists across sign-out/sign-in in same browser profile.
- [ ] Confirm automatic mode is recomputed when explicit preference is cleared.
- [ ] Confirm changing theme does not reset density.
- [ ] Confirm changing density does not reset theme.
- [ ] Confirm startup does not flash wrong density for prolonged time.

### Regression and tests
- [ ] Confirm bUnit tests cover settings route and option rendering.
- [ ] Confirm bUnit tests cover nav settings entry visibility.
- [ ] Confirm bUnit tests cover density options text and default mapping.
- [ ] Confirm any existing theme-related tests remain green.
- [ ] Confirm report page tests remain green after global token changes.
- [ ] Confirm no API integration tests require updates.
- [ ] Confirm `dotnet test tests/FamilyFinances.Web.Tests -c Release` passes.
- [ ] Confirm release notes/docs mention new settings behavior and limits.
