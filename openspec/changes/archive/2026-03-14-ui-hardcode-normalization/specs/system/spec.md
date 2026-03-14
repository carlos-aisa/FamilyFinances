## MODIFIED Requirements

### Requirement: Premium Styling SHALL Be Implemented Through Shared Primitives
Cross-page premium styling MUST be implemented through shared primitives, MUST cover all Web UI surfaces, and MUST avoid one-off page-level visual forks or newly hardcoded presentation literals.

#### Scenario: Shared primitives are applied before page-specific overrides
- **WHEN** premium styling is implemented or refactored in the Web UI
- **THEN** shared tokenized primitives MUST be the primary mechanism for cards, tables, tabs, panel framing, control sizing, and chart surfaces
- **AND** page-level custom overrides MUST be limited to documented exceptions only

#### Scenario: Regression guardrails prevent style hardcode drift
- **WHEN** frontend changes are validated in automated test gates
- **THEN** newly introduced disallowed hardcoded visual literals in protected paths MUST fail validation
- **AND** shared primitive consumption requirements MUST remain enforceable through deterministic checks
