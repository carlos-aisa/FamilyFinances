## MODIFIED Requirements

### Requirement: User-Facing Formatting SHALL Follow Active Culture
Date and currency presentation in targeted components MUST follow active `CurrentCulture`/`CurrentUICulture` semantics, with EUR as the fixed currency identity for monetary amounts.

#### Scenario: Date formatting follows selected language
- **WHEN** the user language is `es-ES`
- **THEN** month/day names in targeted pages MUST render in Spanish

#### Scenario: Date formatting changes after switch
- **WHEN** the user switches language to `en-US`
- **THEN** month/day names in targeted pages MUST render in English after immediate refresh

#### Scenario: Currency formatting keeps culture separators and EUR identity
- **WHEN** amounts are displayed in targeted pages/components under any supported culture
- **THEN** formatting MUST reflect selected culture conventions for numeric separators and placement
- **AND** the rendered currency symbol identity MUST be EUR (`€`) rather than culture-default foreign symbols

## ADDED Requirements

### Requirement: Localization Changes SHALL NOT Introduce Foreign Currency Symbols In Monetary UI
Localization behavior MUST NOT render "$" or other foreign currency symbols for domain monetary values because ledger currency is single-currency EUR.

#### Scenario: English UI still renders EUR currency symbol
- **WHEN** active culture is `en-US`
- **THEN** monetary values in targeted components MUST still render with `€`
- **AND** only language text and numeric separators MAY vary per culture

#### Scenario: Spanish UI renders EUR currency symbol
- **WHEN** active culture is `es-ES`
- **THEN** monetary values in targeted components MUST render with `€`
- **AND** no culture fallback path MUST render `$` for domain monetary amounts
