## 1. Insight domain and contracts

- [ ] 1.1 Add insight DTOs for Pareto ranking, concentration indicators, and anomaly results.
- [ ] 1.2 Implement Application-layer insight services/calculators with deterministic formulas and threshold metadata.
- [ ] 1.3 Add repository/query support for required historical baselines and grouped aggregates.

## 2. API exposure

- [ ] 2.1 Add reporting insight endpoints (or endpoint extensions) for expense/income Pareto and concentration outputs.
- [ ] 2.2 Add anomaly endpoint/contract for monthly group anomaly evaluation.
- [ ] 2.3 Add API validation and error-handling paths for unsupported filters or insufficient data contexts.

## 3. Web integration

- [ ] 3.1 Add insight panels/cards to relevant reporting pages with collapsible details.
- [ ] 3.2 Display contribution percentages, top-N coverage, and denominator context explicitly.
- [ ] 3.3 Display anomaly badges/messages with explanation and insufficient-history state.

## 4. Tests and quality gates

- [ ] 4.1 Add Application tests for ranking order, percentage math, and anomaly threshold behavior.
- [ ] 4.2 Add API integration tests covering success, invalid input, and insufficient-history responses.
- [ ] 4.3 Add Web tests validating insight panel rendering and semantic text for explainability.

## 5. Validation and release preparation

- [ ] 5.1 Run impacted test suites in Release configuration.
- [ ] 5.2 Verify insight values against known fixture datasets and manual spot-check calculations.
- [ ] 5.3 Document `0.9.5` insight formulas, thresholds, and user-facing interpretation notes.
