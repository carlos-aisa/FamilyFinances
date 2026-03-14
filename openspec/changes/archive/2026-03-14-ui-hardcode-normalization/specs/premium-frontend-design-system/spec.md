## MODIFIED Requirements

### Requirement: Web UI SHALL Define A Dark-First Premium Token Contract
The Web UI MUST define a centralized token contract for premium presentation, including semantic color roles, typography, spacing, radius, elevation, and chart palette values, and that contract MUST be reusable across the full application surface.

#### Scenario: Premium tokens are centralized and reusable
- **WHEN** Web UI styles are loaded for authenticated and unauthenticated surfaces
- **THEN** premium visual values MUST come from shared token definitions rather than page-specific hardcoded values
- **AND** the same token contract MUST be consumable by layout, cards, tables, and chart surfaces across dashboard, reports, accounts, transactions, history, payees, settings, and login

#### Scenario: Dark-first baseline is deterministic
- **WHEN** no persisted theme preference exists
- **THEN** the visual baseline MUST resolve to dark mode
- **AND** premium token values for dark surfaces and text contrast MUST be applied without requiring user action

### Requirement: Chart Presentation SHALL Be Tokenized And Semantically Consistent
Chart rendering for monthly and annual report surfaces MUST use shared tokenized styling for axes, grids, tooltips, semantic dataset colors, and composition legend sizing contracts.

#### Scenario: Chart primitives use shared visual semantics
- **WHEN** a report chart is rendered
- **THEN** axis, grid, and tooltip styling MUST use shared premium chart tokens
- **AND** chart chrome MUST remain consistent with report card and panel styling
- **AND** chart containers and legends MUST consume shared sizing tokens rather than per-view literals

#### Scenario: Income and expense series keep semantic color mapping
- **WHEN** charts render income and expense datasets
- **THEN** income-related series MUST keep success semantics
- **AND** expense-related series MUST keep danger semantics
- **AND** semantic series colors MUST be resolved through shared palette helpers rather than page-level literal hex assignments

### Requirement: Frontend Documentation SHALL Define Premium Design Governance
Project documentation MUST describe the premium design contract so future UI changes remain consistent, including explicit anti-hardcode governance and style ownership rules.

#### Scenario: Documentation includes token and usage rules
- **WHEN** maintainers consult frontend styling documentation
- **THEN** docs MUST define token categories, approved semantic mappings, and reusable primitives
- **AND** docs MUST include explicit do/don't guidance to prevent one-off style regressions
- **AND** docs MUST define ownership boundaries for global token files, shared style layers, and component-scoped style files
