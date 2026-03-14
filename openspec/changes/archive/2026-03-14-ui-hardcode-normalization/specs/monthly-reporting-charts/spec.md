## MODIFIED Requirements

### Requirement: Monthly Report Charts SHALL Use A Shared Premium Visual Contract
Month-level charts MUST render with a shared premium visual contract for chart container, typography, axes, grid, tooltip presentation, and semantic palette resolution sourced from shared token governance.

#### Scenario: Month-focused chart panels use premium chart framing
- **WHEN** a monthly report chart is rendered in integrated report tabs
- **THEN** chart container, title/subtitle, and action controls MUST use shared premium chart panel styles
- **AND** chart framing MUST remain consistent across monthly chart contexts
- **AND** chart panel sizing and legend sizing MUST resolve from shared tokens rather than per-view literals

#### Scenario: Monthly chart axis and tooltip styling are tokenized
- **WHEN** monthly chart canvases are rendered
- **THEN** axis ticks, grid lines, and tooltip visuals MUST derive from shared design tokens
- **AND** style choices MUST preserve readability in dark mode by default
- **AND** runtime fallback values MUST be sourced through shared semantic chart mappings

### Requirement: Monthly Income-Expense Charts SHALL Be Reusable In Dashboard And Report Contexts
Monthly Income vs Expense chart contracts MUST be reusable across Dashboard and report pages without semantic divergence and without style-contract divergence.

#### Scenario: Dashboard and report render equivalent monthly semantics
- **WHEN** monthly Income vs Expense charts are rendered in Dashboard and Economic State contexts
- **THEN** both views MUST represent equivalent month-relative evolution semantics for the same period
- **AND** both views MUST derive rendered values from the same underlying monthly source values
- **AND** chart series meaning MUST remain consistent across contexts
- **AND** both contexts MUST resolve semantic series colors through shared palette helpers

#### Scenario: Reused chart contract preserves deterministic ordering
- **WHEN** monthly chart datasets are loaded for a selected month
- **THEN** points MUST render in payload order
- **AND** frontend style layers MUST NOT alter day ordering or value interpretation

## ADDED Requirements

### Requirement: Month-Focused Daily Evolution SHALL Normalize Against Month Opening Balance
Month-focused daily evolution charts MUST use month opening balance as the baseline so evolution starts at zero while preserving daily movement semantics.

#### Scenario: Day-1 movement is preserved while baseline starts at zero
- **WHEN** a monthly chart includes transactions on day 1
- **THEN** the rendered value at day 1 MUST include day-1 movement relative to month opening balance
- **AND** the chart MUST NOT suppress day-1 movement by subtracting the first rendered point as baseline

#### Scenario: Opening balance metadata is additive and deterministic
- **WHEN** monthly chart payloads are prepared for frontend rendering
- **THEN** payloads MAY include additive `OpeningBalanceCents` metadata for baseline normalization
- **AND** this metadata MUST NOT alter point ordering, day buckets, or source value determinism
