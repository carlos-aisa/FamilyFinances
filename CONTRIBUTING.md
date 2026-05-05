# Contributing to FamilyFinances

Thanks for considering a contribution.
This project prioritizes correctness, reproducibility, and maintainable architecture.

## Development Principles

- Keep boundaries explicit across `Domain`, `Application`, `Infrastructure`, `Api`, and `Web`.
- Favor deterministic behavior in financial calculations and reporting.
- Add or update tests for every behavior change.
- Keep pull requests focused and reviewable.

## Getting Started

1. Fork the repository and create a branch from `develop` for regular changes.
2. Use branch names such as:
   - `feat/<short-description>`
   - `fix/<short-description>`
   - `docs/<short-description>`
3. Run:

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

## Commit Conventions

Use conventional commits where possible:

- `feat:` new behavior
- `fix:` bug fix
- `refactor:` non-functional code change
- `test:` tests only
- `docs:` documentation changes
- `chore:` tooling/infra maintenance

Examples:

- `feat(reports): add monthly net trend aggregation`
- `fix(api): validate transfer split balance before commit`

## Pull Request Checklist

Before opening a PR, ensure:

- [ ] Scope is clear and limited.
- [ ] Tests cover the changed behavior.
- [ ] Existing tests still pass.
- [ ] Documentation is updated when behavior changes.
- [ ] Screenshots are included for UI changes.

## Review Expectations

- Every PR should explain the why, not only the what.
- Risky changes should include rollback notes.
- If a change affects persistence or API contracts, call that out explicitly.

## Code of Conduct

Be respectful, precise, and constructive in discussions and reviews.
