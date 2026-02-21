## 1. Canonical metric semantics

- [ ] 1.1 Introduce a canonical reporting metric dictionary in Application reporting layer (stock/flow classification + formula intent).
- [ ] 1.2 Add deterministic mapping helpers from report KPI/chart identifiers to canonical metric semantics.
- [ ] 1.3 Ensure existing reporting DTO usage remains backward compatible while consuming canonical semantics in higher layers.

## 2. Monthly evolution semantic alignment

- [ ] 2.1 Update `MonthlyEvolution` Accounts scope summary cards to use asset-only aggregation when asset series are available.
- [ ] 2.2 Update Accounts scope summary labels to explicit asset semantics (`Latest Asset ...`) and preserve fallback behavior.
- [ ] 2.3 Ensure non-equivalent metric disclaimer is visible in monthly evolution when stock/flow comparisons are presented.

## 3. Cross-report label consistency

- [ ] 3.1 Align report page labels and helper text to canonical naming (`Asset`, `Liability`, `Net Worth`, `Period Net Result`) where applicable.
- [ ] 3.2 Review report index descriptions and in-page informational text for semantic consistency with canonical definitions.
- [ ] 3.3 Remove or update ambiguous phrasing that implies equivalence between flow and stock metrics.

## 4. Test coverage and validation

- [ ] 4.1 Add/update Web report page tests validating KPI labels map to expected semantics and disclaimers appear where required.
- [ ] 4.2 Add/update Application/API tests validating semantic mapping behavior and preventing label-formula drift regressions.
- [ ] 4.3 Run reporting-focused test suites and full web tests in Release configuration.

## 5. Documentation and release readiness

- [ ] 5.1 Update OpenSpec system/monthly evolution specs if implementation details require final requirement wording adjustment.
- [ ] 5.2 Update release notes/changelog documentation for `0.9.1` semantic alignment behavior changes.
- [ ] 5.3 Validate CI health: local build, tests, and docs consistency check before merge.
