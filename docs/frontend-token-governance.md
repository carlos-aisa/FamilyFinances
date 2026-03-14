# Frontend Token Governance

## Purpose
This document defines the canonical visual token ownership model for the Web UI and the anti-hardcode guardrails that must be respected in all future frontend changes.

## Canonical Sources Of Truth
- Global primitives and semantic aliases: `src/FamilyFinances.Web/wwwroot/css/ui-tokens.css`
- Shared token consumption and utility classes: `src/FamilyFinances.Web/wwwroot/css/app.css`
- Premium theme-specific overlays: `src/FamilyFinances.Web/wwwroot/css/premium-theme.css`

## Ownership Rules
1. Define global primitives only in `ui-tokens.css`.
2. Shared styles (`app.css`) MUST consume tokens and MUST NOT introduce new global primitive values.
3. Theme files (`premium-theme.css`) MAY override semantic tokens for theme behavior only.
4. Component styles (`Components/**/*.razor.css`) SHOULD use shared classes/tokens and MUST avoid redefining global primitives.
5. Razor markup SHOULD NOT use static inline style literals.

## Chart Palette Governance
- Semantic keys:
  - `income` -> success semantics
  - `expense` -> danger semantics
  - `balance` -> info/balance semantics
  - `neutral` -> primary neutral chart semantics
- C# palette source:
  - `src/FamilyFinances.Web/Features/Reports/Charts/ChartSemanticPalette.cs`
- Runtime chart token consumption:
  - `src/FamilyFinances.Web/wwwroot/js/reportCharts.js`

## Inline Style Policy
- Forbidden: static inline presentation literals (fixed widths, colors, radius, spacing).
- Allowed only as scoped dynamic custom-property assignment:
  - `style="--ff-progress-width:@(percentage)%"`
  - `style="--ff-slice-color:{slice.ColorHex}"`
- Any new dynamic inline pattern must be explicitly allowlisted in tests.

## Do / Don't
- Do: add or update token names in `ui-tokens.css` before touching page/component styling.
- Do: prefer shared utility classes for repeated layout constraints.
- Do: resolve chart colors through `ChartSemanticPalette` and indexed fallback helpers.
- Don't: add new hardcoded hex values in protected frontend files.
- Don't: reintroduce one-off style patches in page components.
- Don't: bypass token files for chart fallback theme values.

## Guardrail Tests
- `tests/FamilyFinances.Web.Tests/Features/Layout/HardcodedStyleGuardTests.cs`
- `tests/FamilyFinances.Web.Tests/Features/Layout/PremiumThemeCssTests.cs`

These tests are part of the expected quality gate for UI style consistency.
