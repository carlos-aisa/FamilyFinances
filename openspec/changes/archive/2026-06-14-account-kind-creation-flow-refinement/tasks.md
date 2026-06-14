## 1. Accounts creation flow refinement

- [x] 1.1 Extract or introduce a reusable Accounts kind selector interaction that keeps `Kind` selection filtered by the current account `Nature`.
- [x] 1.2 Remove the full custom-kind administration block from the primary account creation form in `src/FamilyFinances.Web/Components/Pages/Accounts/AccountsListPage.razor`.
- [x] 1.3 Add the compact inline `New kind` interaction to the account creation flow so users can create a missing custom kind without leaving the form.
- [x] 1.4 Ensure inline kind creation inherits the current account `Nature`, refreshes the available options, and auto-selects the created kind on success.
- [x] 1.5 Preserve fallback behavior so changing `Nature` recomputes compatible kinds and resets invalid selections to the default compatible kind.

## 2. Secondary kind management surface

- [x] 2.1 Add a secondary `Manage kinds` entry point inside the Accounts feature with lower visual priority than the primary account creation action.
- [x] 2.2 Move low-frequency custom kind administration behavior behind that entry point, including custom-kind listing, enable/disable, and delete actions.
- [x] 2.3 Keep the management surface aligned with existing kind-catalog API behaviors and current account-usage safeguards.
- [x] 2.4 Apply the agreed visual hierarchy so auxiliary kind actions remain compact and do not clutter the primary account creation layout.

## 3. Behavioral test coverage

- [x] 3.1 Update Accounts web tests to verify the primary account creation form no longer renders full kind-management controls by default.
- [x] 3.2 Add or update tests covering nature-filtered kind selection and default fallback after `Nature` changes.
- [x] 3.3 Add or update tests covering inline custom-kind creation success, including inherited `Nature` and auto-selection of the newly created kind.
- [x] 3.4 Add or update tests covering inline custom-kind creation failure while preserving the rest of the account form state.
- [x] 3.5 Add or update tests covering access to the secondary `Manage kinds` surface and its continued full administration behavior.

## 4. Validation and documentation checks

- [x] 4.1 Review implementation against `openspec/changes/account-kind-creation-flow-refinement/design.md` and both delta specs to confirm behavior alignment.
- [x] 4.2 Run focused web test coverage for the touched Accounts scenarios and resolve any regressions in the affected scope.
- [x] 4.3 Run broader validation required by the touched scope, at minimum build plus the relevant web test project.
- [x] 4.4 Update any implementation-adjacent documentation if the final UI behavior or interaction naming differs materially from the current approved OpenSpec text.
