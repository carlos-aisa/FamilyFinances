## ADDED Requirements

### Requirement: Reports Catalog SHALL Use Updated Report Naming For Account Analysis
Report entry naming MUST reflect the updated account-analysis terminology in navigation and report index surfaces.

#### Scenario: Monthly summary naming is replaced by account analysis naming
- **WHEN** report entries are rendered in navigation/report catalog contexts
- **THEN** the previous label `Monthly Summary` MUST be replaced with the configured localized equivalent of `Account Analysis`
- **AND** route targets and report behavior MUST remain unchanged

### Requirement: Report Footer Metadata Tags SHALL Be Semantically Aligned
Report footer tags/badges MUST only be displayed when their meaning is explicit and aligned with rendered report content.

#### Scenario: Unknown or mismatched footer tags are not shown
- **WHEN** a report page computes footer metadata tag content that cannot be mapped to explicit semantics
- **THEN** that tag MUST NOT be rendered
- **AND** misleading unmatched footer labels MUST be prevented

#### Scenario: Visible footer tags have explicit localized meaning
- **WHEN** a report page renders a footer metadata tag
- **THEN** the tag text MUST map to explicit localized semantics that match the active report content
- **AND** the same semantic mapping MUST be applied consistently across report pages
