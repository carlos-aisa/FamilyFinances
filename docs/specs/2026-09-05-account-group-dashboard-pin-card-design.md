# Account Group Dashboard Pin Card Indicator

## Goal

Make the dashboard-monitoring state visible from the account-group list without adding controls or changing the existing group-management flow.

## Design

Each account-group card will conditionally render a compact, localized status badge in its header when `IsDashboardPinned` is true. The badge will use the established positive semantic styling and a pin icon, with text equivalent to “Shown on dashboard”.

Cards for groups that are not pinned will render no placeholder, muted alternative, or control. This keeps the list focused while allowing the enabled state to be scanned quickly.

## Data Flow

`AccountGroupsListPage` already receives `IsDashboardPinned` in `AccountGroupDto` from its existing list API call. The presentation change consumes that property only; it introduces no new API request, local state, persistence behavior, or navigation.

## Accessibility and Localization

The badge is text-bearing rather than icon-only, so its meaning is available to assistive technology. The label is supplied by the shared English and Spanish resource files.

## Testing

Update the component test for the account-group list to assert that a pinned group renders the badge and an unpinned group does not. Existing list behavior remains covered by its regression tests.
