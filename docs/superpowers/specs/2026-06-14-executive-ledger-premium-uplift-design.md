# Executive Ledger Premium Uplift Design

Date: 2026-06-14
Status: Proposed
Owner: Codex + user

## Summary

FamilyFinances already has a usable token and premium-theme foundation, but the current experience still feels too close to Bootstrap defaults. The goal of this design is to give the application a more premium, executive-grade visual identity without changing product behavior.

The chosen direction is **Executive Ledger**:

- serious and financially credible,
- dense but calm,
- premium through hierarchy and restraint rather than decorative effects,
- consistent across light and dark modes,
- focused first on shell, shared patterns, and the four highest-visibility feature areas:
  - Dashboard,
  - Accounts,
  - Quick Entry,
  - Reports.

Charts are intentionally deferred to a second phase so the first pass can focus on overall application feel.

## Problem Statement

The current application does not feel premium in a single obvious place; it feels generically utilitarian across the whole experience.

The main causes are:

- shell and navigation still read as Bootstrap-derived,
- page headers are too lightweight and do not anchor each screen,
- cards and panels do not establish a strong surface hierarchy,
- tables and forms feel serviceable rather than refined,
- visual language exists in isolated details rather than as a cohesive system.

This creates the impression of "theme overrides on top of a framework" instead of a deliberately designed product.

## Goals

- Establish a distinctive **Executive Ledger** visual language.
- Make light and dark modes feel like siblings within the same design system.
- Improve perceived quality across the full application, not just on one hero page.
- Create reusable shared patterns for shell, page headers, surfaces, controls, and data presentation.
- Apply the first premium uplift to:
  - `Dashboard`
  - `Accounts`
  - `Quick Entry`
  - `Reports`

## Non-Goals

- No product-behavior redesign.
- No chart redesign in this phase.
- No broad frontend architecture rewrite.
- No introduction of a new component framework.
- No decorative motion system or heavy visual effects for their own sake.

## Chosen Design Direction

### Tone

The application should feel:

- executive,
- trustworthy,
- structured,
- analytical,
- calm under heavy data density.

It should **not** feel:

- playful,
- startup-generic,
- soft consumer lifestyle,
- highly ornamental,
- glassy for the sake of trendiness.

### Premium Definition

In this project, "premium" means:

- stronger hierarchy,
- more intentional spacing,
- better typography and number presentation,
- cleaner grouping of actions and data,
- more controlled surfaces and borders,
- less visual noise.

Premium does **not** mean more gradients, more shadows, or more animation by default.

## Visual System Strategy

The uplift is organized around five shared visual blocks.

### 1. Shell

Scope:

- `MainLayout`
- `NavMenu`
- app content shell
- sidebar structure
- global page spacing

Intended outcome:

- the shell feels architectural and product-defining,
- navigation reads as part of a serious finance tool,
- content sits inside a more deliberate frame with stronger spatial rhythm.

Key changes:

- refine sidebar proportions, spacing, and active-state treatment,
- reduce the "list of links" feeling,
- improve grouping and visual weighting of navigation clusters,
- strengthen the main content shell with more deliberate horizontal and vertical rhythm,
- ensure the shell works equally well in dark and light themes.

### 2. Page Headers

Scope:

- page titles
- subtitles
- top-level actions
- contextual summary areas where needed

Intended outcome:

- each screen should open with a clear sense of purpose,
- titles and actions should feel intentionally composed rather than loosely stacked.

Key changes:

- introduce a stronger page-header pattern,
- group title/subtitle/actions into a stable structure,
- support optional summary or context chips,
- make action hierarchies visually unambiguous.

### 3. Surfaces

Scope:

- cards
- panels
- section containers
- empty states
- grouped blocks

Intended outcome:

- panels feel like part of a product system rather than default cards,
- important content areas gain weight and readability.

Key changes:

- define clearer surface tiers,
- reduce inconsistent border/shadow usage,
- improve radii, padding rhythm, and header/body relationships,
- make container hierarchy visible without becoming noisy.

### 4. Controls

Scope:

- buttons
- tabs
- form controls
- selects
- filter rows
- chips and minor utilities

Intended outcome:

- controls feel refined and deliberate,
- forms and filters look product-grade rather than scaffolding-grade.

Key changes:

- unify heights, paddings, label spacing, and inline grouping rules,
- improve primary/secondary/tertiary action contrast,
- refine tabs so they feel like part of the premium system,
- make filter bars look composed rather than assembled.

### 5. Data Presentation

Scope:

- tables
- numeric emphasis
- badges
- statuses
- row density
- scanability

Intended outcome:

- dense financial information becomes easier to scan,
- tabular areas feel authoritative rather than generic.

Key changes:

- improve table header treatment,
- strengthen numeric alignment and emphasis,
- improve zebra/hover/border logic,
- define more premium badge and status styling,
- make action cells and summary rows more intentional.

## Theme Philosophy: Light and Dark

The application already supports light and dark modes, and that must remain true after the uplift.

The new rule is:

- **same structural design language**
- **different atmosphere**

### Dark Mode

Dark mode should feel:

- more cockpit-like,
- more immersive,
- more executive,
- slightly more dramatic in depth and contrast.

### Light Mode

Light mode should feel:

- analytical,
- crisp,
- professional,
- premium without becoming soft or airy.

### Shared Constraints

Both modes must share:

- the same spacing system,
- the same component proportions,
- the same action hierarchy,
- the same typography logic,
- the same information architecture.

This avoids creating the impression of two separate UI systems.

## Area-by-Area Application

### Dashboard

The dashboard should become the clearest expression of the new direction.

Desired feel:

- a control center, not a loose mosaic,
- stronger KPI presence,
- cleaner grouping,
- more deliberate rhythm between summary, detail, and drill-down areas.

Focus:

- KPI cards,
- dashboard section headers,
- composition and spacing between panels,
- improved visual priority for the most important financial signals.

### Accounts

Accounts should feel sharper and more product-grade, especially in forms and data grids.

Desired feel:

- high-trust operational workspace,
- clear relationship between filters, account list, and create/edit surfaces,
- stronger emphasis on financial values and account state.

Focus:

- create/edit form styling,
- account list table styling,
- action hierarchy,
- filter and management surfaces.

### Quick Entry

Quick Entry should keep its speed while feeling more specialized and premium.

Desired feel:

- a focused tool,
- less like a generic form page,
- more like a dedicated workflow surface.

Focus:

- mode switching,
- action emphasis,
- card composition,
- presets and helper controls.

### Reports

Reports should feel more like an analytical suite than a collection of cards and pages.

Desired feel:

- clear report hierarchy,
- calmer, more serious panels,
- stronger framing around metrics and report navigation.

Focus:

- reports index cards,
- report page headers,
- analytical panel styling,
- shared table/report container treatment.

Charts remain out of scope for this phase, but the report surfaces should be designed so a later chart uplift can slot in naturally.

## Implementation Shape

Recommended implementation order:

1. Shell and shared visual primitives
2. Shared header/surface/control/data patterns
3. Dashboard
4. Accounts
5. Quick Entry
6. Reports
7. Charts in a later follow-up change

This sequence is important because the premium feel should come from the system first, not from isolated page-specific polish.

## Likely Files Affected

Shared styling:

- `src/FamilyFinances.Web/wwwroot/css/ui-tokens.css`
- `src/FamilyFinances.Web/wwwroot/css/premium-theme.css`
- `src/FamilyFinances.Web/wwwroot/css/app.css`

Layout:

- `src/FamilyFinances.Web/Components/Layout/MainLayout.razor`
- `src/FamilyFinances.Web/Components/Layout/MainLayout.razor.css`
- `src/FamilyFinances.Web/Components/Layout/NavMenu.razor`
- `src/FamilyFinances.Web/Components/Layout/NavMenu.razor.css`

Primary feature areas:

- `src/FamilyFinances.Web/Components/Pages/Dashboard/DashboardPage.razor`
- `src/FamilyFinances.Web/Components/Pages/Accounts/AccountsListPage.razor`
- `src/FamilyFinances.Web/Components/Pages/QuickEntry/QuickEntryPage.razor`
- `src/FamilyFinances.Web/Components/Pages/Reports/ReportsIndexPage.razor`
- `src/FamilyFinances.Web/Components/Pages/Reports/*.razor`

Shared components likely to matter:

- header-like shared components already in use,
- account selector / quick entry surfaces,
- shared tables, panel wrappers, and utility classes where applicable.

## Risks and Mitigations

### Risk: Premium uplift turns into isolated page polish

Mitigation:

- implement shell and shared patterns first,
- treat pages as consumers of a system.

### Risk: Dark mode improves more than light mode

Mitigation:

- define parallel light-mode semantics up front,
- validate every major shared pattern in both themes before page rollout.

### Risk: Premium becomes less usable

Mitigation:

- preserve density where it supports productivity,
- prioritize clarity over decoration,
- keep motion subtle and optional.

### Risk: Too much page-specific styling

Mitigation:

- prefer reusable classes and shared component patterns,
- avoid one-off page CSS unless truly necessary.

## Validation Criteria

The uplift is successful when:

- the app no longer reads as generic Bootstrap with custom colors,
- page headers feel anchored and deliberate,
- sidebar and shell feel like part of a serious finance product,
- tables/forms/filters feel cohesive and product-grade,
- the four target feature areas visibly belong to the same premium system,
- light and dark modes both feel intentional and related.

## Phase 2 Preview

A follow-up design pass should revisit charts and analytical visualization:

- chart palettes,
- gridlines,
- legends,
- tooltip treatment,
- line/bar thickness,
- chart framing inside report panels.

That work is intentionally deferred so the current phase can focus on fixing the application's overall premium feel first.
