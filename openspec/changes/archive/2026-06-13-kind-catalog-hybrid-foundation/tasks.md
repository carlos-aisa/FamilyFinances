## 1. Catalog Data Model and Migration

- [x] 1.1 Add domain/application model for account kind catalog with required fields (`Id`, `Key`, `Name`, `IsSystem`, `IsActive`, `SortOrder`).
- [x] 1.2 Add EF Core configuration and migration for the new kind catalog table plus account `KindId` foreign key.
- [x] 1.3 Seed predefined system kinds with deterministic keys and ordering.
- [x] 1.4 Backfill existing accounts from legacy enum kind values to seeded catalog `KindId` using deterministic mapping.
- [x] 1.5 Remove enum-backed account kind persistence as source of truth after migration validation.

## 2. API and Application Contract Updates

- [x] 2.1 Update account DTOs/requests/handlers to use catalog-backed kind identity semantics.
- [x] 2.2 Validate account create/update flows reject unknown or inactive kind selections with deterministic errors.
- [x] 2.3 Keep account lifecycle operations unchanged outside kind identity migration scope.
- [x] 2.4 Update API OpenSpec/OpenAPI artifacts if account contract shape changes.

## 3. Web UX Unification (System + Custom Kind)

- [x] 3.1 Implement unified kind source for account creation/edit selectors (predefined + custom in one list).
- [x] 3.2 Add global custom kind management entry points required to create and activate/deactivate custom kinds.
- [x] 3.3 Replace duplicated enum/switch kind label mappings with centralized catalog-driven label resolution.
- [x] 3.4 Ensure quick-entry global account search includes catalog-driven kind labels for both predefined and custom kinds.

## 4. Tests and Regression Coverage

- [x] 4.1 Add/adjust domain and application tests for kind catalog invariants and key uniqueness rules.
- [x] 4.2 Add infrastructure integration tests for migration backfill and account-kind foreign key behavior using relational provider.
- [x] 4.3 Add/adjust API integration tests for account create/read behavior with catalog-backed kind identity.
- [x] 4.4 Add/adjust web tests for unified kind selectors and quick-entry search behavior with custom kind labels.

## 5. Validation and Documentation

- [x] 5.1 Run solution build and relevant test projects covering domain/application/infrastructure/api/web impacts.
- [x] 5.2 Run `openspec validate kind-catalog-hybrid-foundation --strict` and resolve any artifact issues.
- [x] 5.3 Update implementation documentation in English for kind catalog semantics, migration notes, and deferred bank-import scope.

## 6. Follow-up Scope (Nature Binding + Existing Account Reclassification)

- [x] 6.1 Bind custom account kinds to a required `Nature` and enforce compatibility in account creation.
- [x] 6.2 Add account kind update operation for existing accounts with active/compatibility validation.
- [x] 6.3 Add migration backfill for `AccountKinds.Nature` using deterministic system mapping and existing account references.
- [x] 6.4 Add/adjust tests and OpenAPI/OpenSpec artifacts for the new behavior.

## 7. Follow-up Scope (Custom Kind Cleanup + Selector Ordering)

- [x] 7.1 Add account kind delete operation restricted to non-system kinds that are not referenced by accounts.
- [x] 7.2 Expose delete-kind API endpoint and update API clients.
- [x] 7.3 Add UI custom kind delete action with in-use guard feedback.
- [x] 7.4 Order account kind selectors by visible localized label in account create/edit flows.
- [x] 7.5 Add/adjust application, API integration, and web tests plus OpenAPI/OpenSpec docs for this behavior.
