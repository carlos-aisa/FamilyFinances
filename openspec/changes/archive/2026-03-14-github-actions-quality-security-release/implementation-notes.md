<!--
Migration checklist captured from legacy workflow:
Source: d:/Programacion/FamilyFinances/.github/workflows/ci.yml

- [x] Legacy build-and-test responsibilities identified
  - checkout
  - setup-dotnet
  - restore
  - build
  - test

- [x] Legacy reporting-release-gate responsibilities identified
  - API reporting integration filter tests
  - Web reporting filter tests
  - Application reporting filter tests

- [x] Legacy windows-distribution responsibilities identified
  - version extraction
  - Windows dist build script execution
  - dist content verification
  - ZIP smoke testing
  - artifact upload
  - release publish
  - ZIP cleanup

- [x] Migration intent documented
  - quality/security checks split into dedicated workflows
  - release packaging moved to tag-only flow
  - cleanup moved before publish with keep_count=2
-->

## Operational Notes

- Branch protection policy depends on repository visibility/licensing:
  - `main` on public repo with GHAS/Code Security: require `ci-quality`, `dependency-review`, and `analyze`.
  - `main` on private repo without GHAS/Code Security: require `ci-quality` only.
  - `develop`: checks are informational and not required blockers.
