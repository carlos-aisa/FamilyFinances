# Account-group monthly account detail design

## Goal

The Account Group Totals State Evolution view currently repeats the account-group annual list in its right-hand Evolution panel. Replace that duplicate with the accounts that compose one explicitly selected group and month from the historical list on the left.

## Interaction

1. The user expands a group in the left summary table with the existing View months button. That button continues to only show or hide the monthly history.
2. The user clicks a monthly row in that expanded history.
3. The clicked row establishes a local selection of `(account group, month)`.
4. The right-hand Evolution panel shows a compact account list for that group and month. It contains Account, Month balance, and Year to date.
5. Until a historical month is selected, the right-hand panel displays an explicit selection prompt rather than duplicate group information.

`Month balance` maps to the selected account point's existing `DeltaVsPreviousMonthCents`; `Year to date` maps to its existing `DeltaVsYearStartCents`. The display is derived from the existing account-group membership lookup and annual account evolution response. No report calculation, API, or DTO changes are required.

## Boundaries

- The left group summary and its expand/collapse behavior remain unchanged.
- The Composition panel remains available and unchanged.
- The focused-month daily account-group comparison remains unchanged.
- Remove the account movement link from this flow. It is deferred to a future generic navigation change.
- The Dashboard group list and the Account State list are outside this increment.

## Data flow

The host already loads the annual group evolution report, account-group details, and the selected-year context. It will additionally use the existing annual account evolution report to obtain account-level points for the selected month. The selected group details supply the account ids that belong to the group.

The right-hand list filters account series by those ids, finds their selected month point, preserves all member accounts even if a point is unavailable, and orders the available accounts by absolute monthly balance and then name. Existing financial colour semantics remain supplementary to signed values.

## Edge cases

- If group membership or account evolution cannot be loaded, show the existing report error treatment in the detail panel without disrupting the left table.
- If a member account has no point for the selected month, display an unavailable value instead of zero.
- Selecting another month or group replaces the detail selection immediately.
- Collapsing a group does not clear its current selection; the detail remains visible until another historical row is selected or the report year changes.
- Changing year clears the selection and returns the right panel to its selection prompt.

## Testing

- Verify the right panel initially displays the selection prompt.
- Verify clicking a historical month selects its group and month, filters to the member accounts, and maps the two monetary columns from that month.
- Verify a second selected row replaces the detail context.
- Verify the existing expand/collapse button still only controls monthly-history visibility.
- Retain coverage for the unchanged composition and daily comparison controls.
