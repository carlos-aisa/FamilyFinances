## Why

Currently, account searches are case-insensitive but accent-sensitive. Users typing "Maria" won't find accounts named "María", "Jose" won't find "José", etc. This creates friction in the UX, especially for Spanish-speaking users with accentuated names. The search should normalize accented characters to improve findability.

## What Changes

- Implement accent-insensitive search normalization for account name searches
- Apply normalization across all search surfaces: Quick Entry, Accounts page, Account Movements, History, and transaction searches
- "María" matches "Maria", "José" matches "Jose", etc.
- Preserve existing case-insensitive behavior
- Use string normalization (NFD + remove diacritics) for consistent implementation

## Release Impact

Type: patch
Rationale: Bug fix improving existing search behavior, fully backward compatible, no API changes

## Capabilities

### New Capabilities
<!-- None - this is a UX improvement to existing search -->

### Modified Capabilities
<!-- This affects search behavior across multiple capabilities but doesn't change requirements, only improves UX. No spec changes needed. -->

## Impact

**Backend:**
- No backend changes needed (search happens client-side in current implementation)

**Frontend:**
- Quick Entry: `_globalAccountSearchQuery` normalization
- Accounts page: Search filter normalization (if exists)
- Account Movements: Search query normalization
- History Movements: Search query normalization
- Transaction search: Query normalization
- Create utility method `NormalizeForSearch(string text)` to centralize logic

**Testing:**
- Add test cases for accent-insensitive matching in Quick Entry
- Add test cases for accent-insensitive matching in other search surfaces
- Verify case-insensitive behavior is preserved
