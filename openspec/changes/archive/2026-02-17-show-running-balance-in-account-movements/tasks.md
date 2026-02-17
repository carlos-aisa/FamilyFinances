## 1. UI Presentation Update

- [x] 1.1 Add a dedicated `Running Balance` column to the movements table in `src/FamilyFinances.Web/Components/Pages/Accounts/AccountMovementsPage.razor`.
- [x] 1.2 Render `movement.RunningBalance` for each row using existing currency formatting behavior.
- [x] 1.3 Add sign-based running-balance styling helper (`positive`, `negative`, `zero`) in `AccountMovementsPage.razor`.

## 2. Behavioral Validation and Test Updates

- [x] 2.1 Verify manually that `/accounts/{id}/movements` shows one running-balance value per movement row and that values evolve across rows.
- [x] 2.2 Verify that movement amount rendering remains unchanged after adding the running-balance column.
- [ ] 2.3 Add or update UI tests covering the running-balance column rendering behavior in the web test project when applicable.

## 3. Build and Documentation Validation

- [x] 3.1 Run build validation for the web project (`dotnet build src/FamilyFinances.Web/FamilyFinances.Web.csproj`).
- [ ] 3.2 If build is blocked by file locks, stop running web processes and rerun the build to completion.
- [x] 3.3 Confirm no API/OpenAPI changes are required for this UI-only behavior change.
