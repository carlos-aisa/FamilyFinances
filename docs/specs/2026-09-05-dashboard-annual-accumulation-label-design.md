# Dashboard Annual Accumulation Labels

## Goal

Use family-oriented Spanish terminology for the two compact dashboard labels that currently use the YTD abbreviation.

## Design

`Dashboard_Kpi_YtdNet` and `Dashboard_PinnedGroups_Ytd` will both display `Acum. anual` in the Spanish resource file. The underlying calculations, DTO property names, API contracts, English resources, and longer explanatory labels remain unchanged.

## Testing

Update the dashboard component test to assert the localized KPI label and add coverage for the pinned-group column header where needed.
