## 1. Movement Card Presentation

- [x] 1.1 Refactor the account-group movement section into the shared report-card layout with a localized title and active-period badge.
- [x] 1.2 Retain localized loading, error, and empty states inside the movement card.
- [x] 1.3 Move the account-breakdown table and its existing CSV action into a dedicated report card with its active-period badge.

## 2. CSV Export

- [x] 2.1 Add a card-local `Exportar CSV` control that is available only when the active filtered result has rows.
- [x] 2.2 Build the CSV from the loaded movement rows using the existing report CSV builder and browser download interop.
- [x] 2.3 Include report context and the displayed movement columns while preserving newest-first row order and use local export-failure feedback.

## 3. Verification

- [x] 3.1 Add bUnit tests for the card header, period badge, export visibility, filename, and CSV content/order.
- [x] 3.2 Run the focused web test suite and the required solution test suite.
- [x] 3.3 Run `openspec validate account-group-movements-csv-export --strict`.

## 4. Account Movement Navigation

- [x] 4.1 Link account-breakdown rows to the account movements page with the active group-report period and context.
- [x] 4.2 Initialize account movement filters from the report context and return to the group report when it is the parent view.
- [x] 4.3 Preserve the account movement view as the immediate parent when opening a transaction or returning from its edit flow.
- [x] 4.4 Add component coverage for link context, applied date range, and the complete Back-navigation chain.
