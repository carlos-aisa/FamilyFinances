## 1. Chart foundations

- [ ] 1.1 Select and integrate the annual chart rendering approach for Blazor (library/wrapper + shared configuration).
- [ ] 1.2 Create reusable reporting chart components (line/multi-series/composition) and dataset adapter helpers.
- [ ] 1.3 Add shared chart empty/loading/error state components for reporting pages.

## 2. Annual chart implementation by use case

- [ ] 2.1 Implement annual evolution chart for core balance/expense/net metrics.
- [ ] 2.2 Implement annual account-group evolution chart bound to group monthly series.
- [ ] 2.3 Implement annual composition charts for expense groups and income groups with percentage normalization.

## 3. Page integration

- [ ] 3.1 Replace `MonthlyEvolution` chart placeholder with functional annual chart panels.
- [ ] 3.2 Position chart sections above corresponding tables and keep existing table behavior unchanged.
- [ ] 3.3 Ensure chart datasets refresh deterministically on year/scope filter changes.

## 4. Tests and regression safety

- [ ] 4.1 Add/extend Web tests asserting chart presence and scope/year rebind behavior.
- [ ] 4.2 Add tests validating chart dataset values equal table source values for sampled months.
- [ ] 4.3 Add tests validating composition charts sum to 100% (within rounding tolerance).

## 5. Validation and documentation

- [ ] 5.1 Run web test suite and impacted API/application tests in Release configuration.
- [ ] 5.2 Document chart semantics and known visualization limitations in reporting docs.
- [ ] 5.3 Validate CI pipeline compatibility for chart dependencies and static assets.
