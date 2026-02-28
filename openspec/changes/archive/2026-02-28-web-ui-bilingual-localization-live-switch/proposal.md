## CRITICAL IMPLEMENTATION CONSTRAINTS

- ? Do not localize API/domain error payloads; this change applies only to the Blazor Web UI.
- ? Do not hardcode new page-level cultures (for example, additional `new CultureInfo("es-ES")` usage).
- ? Do not introduce client-side frameworks or architecture changes outside current Blazor Interactive Server patterns.
- ? Add runtime language selection in the Settings page and make language switching immediate.
- ? Support exactly two cultures in scope: `es-ES` and `en-US`.
- ? Persist the user language choice in the web client and apply it on subsequent visits.
- ? Keep existing business behavior unchanged; only presentation text/formatting should vary by selected language.

## Why

The Web UI currently mixes hardcoded English text, hardcoded Spanish formatting, and invariant formatting. Users cannot choose language, and the interface does not switch language immediately. This causes inconsistent UX and blocks bilingual use.

## What Changes

- Add Web UI runtime localization infrastructure for `es-ES` and `en-US`.
- Add a language selector in the Settings page and persist preference client-side.
- Apply language selection immediately by reloading the current route after switching culture.
- Replace hardcoded UI strings with localized resource entries for shared layout and core transaction/account/report screens.
- Standardize date/currency formatting to selected UI culture instead of mixed hardcoded culture usage.
- Update `lang` metadata behavior in the web app shell to reflect active UI language.
- Add/adjust tests for localization helpers and culture-dependent formatting behavior in the web test project.

### Non-goals

- No localization of API response payloads, domain exceptions, or backend logs.
- No support for additional languages beyond `es-ES` and `en-US` in this change.
- No redesign of navigation/layout, only language-related additions.
- No changes to authorization, routing model, or business rules.

### Rollback Plan

- Revert files under `src/FamilyFinances.Web` related to localization service registration, selector UI, and resource usage.
- Remove new localization resources for this change and restore previous fixed text/formatting behavior.
- Keep API and database unchanged (rollback is Web-only).

## Capabilities

### New Capabilities
- `web-localization`: Runtime language selection, localized Web UI resources, and culture-driven formatting for the Blazor Web app.

### Modified Capabilities
- None.

## Impact

- Affected UI shell and settings entry points: `src/FamilyFinances.Web/Components/App.razor`, `src/FamilyFinances.Web/Components/Pages/Settings/SettingsPage.razor`.
- Affected UI pages/components with mixed hardcoded culture/text (transactions, accounts, reports, dashboard widgets, shared presets/components).
- Affected helper utilities: report/date/money formatting helpers in Web layer.
- Affected tests: `tests/FamilyFinances.Web.Tests` for culture and formatting coverage.
- No API/OpenAPI contract changes.
- No database migration changes.
