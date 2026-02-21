## 1. Chart foundations

- [x] 1.1 Select and integrate the annual chart rendering approach for Blazor (library/wrapper + shared configuration).
- [x] 1.2 Create reusable reporting chart components (line/multi-series/composition) and dataset adapter helpers.
- [x] 1.3 Add shared chart empty/loading/error state components for reporting pages.

## 2. Annual chart implementation by use case

- [x] 2.1 Implement annual evolution chart for core balance/expense/net metrics.
- [x] 2.2 Implement annual account-group evolution chart bound to group monthly series.
- [x] 2.3 Implement annual composition charts for supported scopes (expense-oriented account groups and account nature composition) with percentage normalization.

## 3. Page integration

- [x] 3.1 Replace report chart placeholders with functional annual chart panels in integrated state-evolution tabs.
- [x] 3.2 Position chart sections above corresponding tables and keep existing table behavior unchanged.
- [x] 3.3 Ensure chart datasets refresh deterministically on year/scope filter changes.

## 4. Tests and regression safety

- [x] 4.1 Add/extend Web tests asserting chart presence and scope/year rebind behavior.
- [x] 4.2 Add tests validating chart dataset values equal table source values for sampled months.
- [x] 4.3 Add tests validating composition calculations sum to 100% (within rounding tolerance) for implemented composition views.

## 5. Validation and documentation

- [x] 5.1 Run web test suite and impacted API/application tests in Release configuration.
- [x] 5.2 Document chart semantics and known visualization limitations in reporting docs.
- [x] 5.3 Validate CI pipeline compatibility for chart dependencies and static assets.
