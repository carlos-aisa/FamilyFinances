## Overview

This change hardens formatting and chart readability contracts in three places:

1. Refund quick-entry original-expense list:
- Date values are rendered as `dd/MM/yyyy`.
- Amount badges render in EUR with suffix (`XXX,XX €`).

2. Chart money labels/tooltips:
- Money formatting is normalized to European numeric representation with trailing euro symbol.

3. Chart Y-axis baseline:
- Zero tick/grid line uses stronger visual emphasis than non-zero lines.

## Design Decisions

### Decision 1: Enforce deterministic date display for refund expense picker
- Use a deterministic formatter (`dd/MM/yyyy`) in display paths used by refund original-expense search results.
- Rationale: avoid culture-dependent `MM/dd/yyyy` drift in this high-sensitivity workflow.

### Decision 2: Use euro suffix formatting for all touched money surfaces
- Format numeric magnitude using European separators and append ` €` after the value.
- Preserve sign semantics (`-` and optional `+`) independent of currency suffix placement.
- Rationale: align with user expectation and existing EUR-only ledger model.

### Decision 3: Highlight Y-axis zero using scriptable grid options
- Configure chart grid options with scriptable callbacks for `color`, `lineWidth`, and `borderDash`.
- For tick `0`: use thicker line and stronger color.
- For non-zero ticks: keep existing thin dashed grid style.
- Rationale: fast visual baseline identification without changing data semantics.

## Compatibility

- No DTO, endpoint, or persistence schema changes.
- No sign or value transformations beyond formatting presentation.
- Existing chart dataset ordering and series semantics are unchanged.

## Risks and Mitigations

- Risk: formatting change broadens beyond intended contexts.
- Mitigation: constrain requirements to quick-entry refund picker and chart renderers in this change, plus targeted tests.

- Risk: scriptable grid options differ across chart configurations.
- Mitigation: apply shared helper for Y-axis grid options in line and bar chart renderers.
