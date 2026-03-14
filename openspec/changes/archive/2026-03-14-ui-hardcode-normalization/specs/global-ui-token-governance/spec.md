## ADDED Requirements

### Requirement: Web UI SHALL Define A Canonical Global Token Registry
The Web UI MUST define one canonical registry for global presentation tokens covering typography, spacing, control sizing, border radius, panel sizing, and chart visual primitives.

#### Scenario: Canonical token source is loadable by all Web surfaces
- **WHEN** the Web host loads global styles for authenticated or unauthenticated pages
- **THEN** canonical token definitions MUST be resolved before token consumers
- **AND** dashboard, reports, accounts, transactions, payees, history, settings, and login surfaces MUST be able to consume the same token registry

#### Scenario: Token families are complete and semantically named
- **WHEN** maintainers inspect the global token registry
- **THEN** token groups MUST include typography, spacing, radius, control height, chart container sizing, and semantic chart palette aliases
- **AND** token naming MUST represent semantic intent rather than page-specific usage

### Requirement: Frontend Styling SHALL Enforce Token Ownership Boundaries
Frontend styling MUST enforce explicit ownership boundaries so global primitives, theme overlays, and component-level styles do not duplicate conflicting sources of truth.

#### Scenario: Shared and component style layers follow defined ownership
- **WHEN** maintainers add or modify presentation styles
- **THEN** global primitive values MUST be defined only in canonical token sources
- **AND** component-scoped styles MUST consume tokens without redefining global primitive constants

#### Scenario: Theme overlays do not duplicate base primitives
- **WHEN** dark/light premium theme overrides are defined
- **THEN** theme files MUST override semantic tokens only where theme-specific behavior is required
- **AND** duplicated base primitive definitions outside canonical token sources MUST NOT be introduced

### Requirement: Chart Semantics SHALL Use Shared Palette Resolution
Chart datasets and chart-rendering helpers MUST resolve visual semantics through shared palette contracts rather than page-level hardcoded color literals.

#### Scenario: Semantic dataset roles resolve to centralized palette mappings
- **WHEN** monthly or annual charts are prepared in dashboard or report contexts
- **THEN** series for income, expense, balance, and indexed comparative datasets MUST resolve colors through shared palette helpers
- **AND** page-level literal hex values for those semantic roles MUST NOT be required

#### Scenario: Chart fallback styling remains deterministic
- **WHEN** chart helpers resolve axis, grid, tooltip, and cutoff styling at runtime
- **THEN** fallback values MUST come from shared semantic token mappings
- **AND** chart rendering MUST remain deterministic if optional per-chart overrides are absent

### Requirement: Frontend Build Validation SHALL Detect Hardcoded Style Regressions
The frontend test/validation surface MUST detect reintroduction of disallowed hardcoded presentation values in protected paths.

#### Scenario: Hardcoded color or inline style regressions fail validation
- **WHEN** protected frontend files introduce disallowed hardcoded color literals or inline style patterns
- **THEN** automated validation MUST fail deterministically
- **AND** failure output MUST identify offending file paths and rule categories

#### Scenario: Documented exceptions remain explicit and minimal
- **WHEN** an exception to hardcode detection is required for data-driven rendering
- **THEN** the exception MUST be explicitly allowlisted with scope-limited justification
- **AND** the exception MUST NOT permit unrestricted hardcoded style usage in unrelated files
