## 1. Insight domain and contracts

- [x] 1.1 Add insight DTOs for Pareto ranking, concentration indicators, and anomaly results with explicit dimension (`group` / `payee`).
- [x] 1.2 Implement Application-layer insight services/calculators with deterministic formulas and threshold metadata.
- [x] 1.3 Add repository/query support for required historical baselines and aggregates by account group and payee.

## 2. API exposure

- [x] 2.1 Add reporting insight endpoints (or endpoint extensions) for expense/income Pareto and concentration outputs for both dimensions.
- [x] 2.2 Add anomaly endpoint/contract for monthly group and payee anomaly evaluation.
- [x] 2.3 Add API validation and error-handling paths for unsupported filters or insufficient data contexts.

## 3. Web integration

- [x] 3.1 Add insight panels/cards to relevant reporting pages with collapsible details and dimension toggle (`Groups` / `Payees`).
- [x] 3.2 Display contribution percentages, top-N coverage, and denominator context explicitly.
- [x] 3.3 Display anomaly badges/messages with explanation and insufficient-history state for group and payee insights.

## 4. Tests and quality gates

- [x] 4.1 Add Application tests for ranking order, percentage math, and anomaly threshold behavior in group and payee dimensions.
- [x] 4.2 Add API integration tests covering success, invalid input, and insufficient-history responses for payee and group requests.
- [x] 4.3 Add Web tests validating insight panel rendering, payee dimension toggle, and semantic text for explainability.

## 5. Validation and release preparation

- [x] 5.1 Run impacted test suites in Release configuration.
- [x] 5.2 Verify insight values against known fixture datasets and manual spot-check calculations.
- [x] 5.3 Document `0.9.5` insight formulas, thresholds, and user-facing interpretation notes.
