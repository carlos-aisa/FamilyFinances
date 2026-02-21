## 1. Client preference foundation (JS + CSS tokens)

- [ ] 1.1 Create `src/FamilyFinances.Web/wwwroot/js/ui-preferences.js` with `uiPreferencesHelper` methods: `init`, `getScale`, `setScale`, `clearScalePreference`, `getScaleSource`; use local storage keys `ff_ui_scale` and `ff_ui_scale_source`.
- [ ] 1.2 Update `src/FamilyFinances.Web/Components/App.razor` to include the new script in `<head>` after `js/theme.js` so startup preferences are applied before page interactions.
- [ ] 1.3 Add `src/FamilyFinances.Web/wwwroot/ui-density.css` defining global density tokens and `html[data-ui-scale="small|medium|large|xlarge"]` profiles.
- [ ] 1.4 Update shared style consumption in `src/FamilyFinances.Web/wwwroot/app.css` (or active shared stylesheet) so typography, controls, card paddings, and table paddings consume density tokens.

## 2. Typed preference service and DI wiring

- [ ] 2.1 Add `src/FamilyFinances.Web/State/IUiPreferencesService.cs` with async methods for theme and density operations used by UI components.
- [ ] 2.2 Add `src/FamilyFinances.Web/State/UiPreferencesService.cs` wrapping JS interop calls to `themeHelper` and `uiPreferencesHelper`.
- [ ] 2.3 Register `IUiPreferencesService` in `src/FamilyFinances.Web/Program.cs` with scoped lifetime.
- [ ] 2.4 Refactor `src/FamilyFinances.Web/Components/Layout/NavMenu.razor` to use the typed service for theme read/toggle (remove duplicated raw JS string calls).

## 3. Settings center route and navigation

- [ ] 3.1 Create `src/FamilyFinances.Web/Components/Pages/Settings/SettingsPage.razor` with route `@page "/settings"` and sections: `Appearance`, `Language (planned)`, `Backup/Restore (planned)`.
- [ ] 3.2 In settings appearance section, implement theme controls (dark/light) and density controls (`Small`, `Medium`, `Large`, `XLarge`) with immediate apply behavior.
- [ ] 3.3 Add optional styling file `src/FamilyFinances.Web/Components/Pages/Settings/SettingsPage.razor.css` to keep section spacing and compact readability consistent across density levels.
- [ ] 3.4 Update `src/FamilyFinances.Web/Components/Layout/NavMenu.razor` to add authenticated `Settings` nav item linking to `/settings` while preserving existing menu entries and logout placement.

## 4. Automatic compact behavior and override semantics

- [ ] 4.1 Implement deterministic auto mode logic in `ui-preferences.js`: if no explicit user scale exists and viewport is mobile/low-res, apply `small`; otherwise `medium`.
- [ ] 4.2 Add reset action in `SettingsPage.razor` (`Use automatic size`) that clears explicit preference and reapplies auto-resolved density.
- [ ] 4.3 Display current density source (`Auto` or `User`) in `SettingsPage.razor` so users can understand why a size is active.
- [ ] 4.4 Ensure explicit user-selected density remains stable across reloads and is not overwritten by auto logic.

## 5. Web test coverage updates

- [ ] 5.1 Add `tests/FamilyFinances.Web.Tests/Features/Settings/SettingsPageTests.cs` covering route render, four density options visibility, placeholder sections, and auto/user source text.
- [ ] 5.2 Add/extend nav tests (new file or existing) to verify authenticated users see `Settings` link and unauthenticated users do not.
- [ ] 5.3 Add tests verifying settings controls trigger expected service interactions (theme update, density update, auto reset).
- [ ] 5.4 Re-run and adjust impacted report page tests under `tests/FamilyFinances.Web.Tests/Features/Reports` if global density class/token changes affect expected markup.

## 6. Validation, docs, and release readiness

- [ ] 6.1 Run `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj -c Release` and fix failures.
- [ ] 6.2 Run `dotnet test tests/FamilyFinances.Api.IntegrationTests/FamilyFinances.Api.IntegrationTests.csproj -c Release --filter \"FullyQualifiedName~Reporting\"` to confirm reporting behavior remains unchanged.
- [ ] 6.3 Update documentation/release notes with density levels, local persistence behavior, settings route location, and known limitations for `XLarge` in dense tables.
- [ ] 6.4 Perform manual smoke check on desktop and constrained viewport (browser responsive mode) to validate auto compact default and explicit override persistence.
