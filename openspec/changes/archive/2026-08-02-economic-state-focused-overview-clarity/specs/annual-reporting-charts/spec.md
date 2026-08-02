## MODIFIED Requirements

### Requirement: Reporting UI SHALL Render Annual Composition Charts for Implemented Scopes
The reporting UI MUST display charted percentage composition in annual reporting views where composition is supported. Annual composition charts MUST keep percentage values for pie geometry while presenting each legend entry as a EUR amount.

#### Scenario: Composition legend exposes monetary slice value
- **WHEN** an annual composition chart renders a side legend
- **THEN** each legend value MUST show the slice's EUR amount using the active UI culture
- **AND** pie slices MUST continue to use percentage values whose total is 100% within rounding tolerance
