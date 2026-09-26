## 1. Reporting contract and data retrieval

- [x] 1.1 Add account-group movement DTOs, query, handler, and dependency registration.
- [x] 1.2 Add repository retrieval that aggregates matching member-account splits by transaction and calculates the running net.
- [x] 1.3 Add the authenticated Reports API endpoint and OpenAPI operation.

## 2. Dashboard and report interaction

- [x] 2.1 Link each Dashboard pinned group to its selected-period account-group totals report.
- [x] 2.2 Add the filtered movements table, loading, error, and empty states to the account-group totals report.
- [x] 2.3 Extend transaction origin context so transaction detail and edit navigation return to the same group report state.
- [x] 2.4 Add localized movement table resources in English and Spanish.

## 3. Automated coverage and validation

- [x] 3.1 Add relational API integration coverage for transaction grouping, filters, and running accumulation.
- [x] 3.2 Add Dashboard, group-report, and transaction-navigation component coverage.
- [x] 3.3 Run the full solution test suite and validate the OpenSpec change.
