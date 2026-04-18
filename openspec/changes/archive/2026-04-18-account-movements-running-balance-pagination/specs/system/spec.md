## ADDED Requirements

### Requirement: Account Movements View SHALL Support Navigating Paginated Results
The system MUST let users navigate all filtered account movements, not only the first page.

#### Scenario: User navigates beyond first 50 movements
Given an authenticated user opens `/accounts/{id}/movements` with filters producing more than 50 rows  
When the movements view is rendered  
Then the UI shows pagination controls that allow moving to next and previous pages  
And selecting next page loads additional movements from the same filtered result set

#### Scenario: Filters reset pagination to first page
Given the user is browsing page `N` (`N > 1`) of account movements  
When the user changes date range presets, manual dates, or search criteria and applies filters  
Then the movements request uses page `1` for the new filter set  
And the rendered table reflects the first page of that updated filter set

#### Scenario: View communicates visible range and total
Given account movements are rendered for a paginated result set  
When header/footer counters are shown  
Then the UI displays the visible movement range and total filtered count  
And the user can determine that more pages exist when the visible range does not cover the total count

## MODIFIED Requirements

### Requirement: Account Movements View SHALL Display Running Balance Evolution
The system MUST display the running account balance for each movement row in the account movements list so users can observe balance evolution across the selected period.

#### Scenario: Running balance shown per movement row
Given an authenticated user opens `/accounts/{id}/movements` and movements are returned  
When the movements list is rendered  
Then each movement row displays its `RunningBalance` value in a dedicated running-balance column

#### Scenario: Running balance uses backend-provided value
Given movement data is rendered in the account movements table  
When the running-balance value is displayed  
Then the value comes from `AccountMovementDto.RunningBalance` without frontend recomputation

#### Scenario: Running balance formatting remains monetary
Given a running-balance value is displayed  
When the value is rendered  
Then it is formatted as currency with sign-preserving semantics consistent with movement amount formatting

#### Scenario: Running balance remains correct across pages
Given account movements for the selected filters span multiple pages  
When any page of `/api/v1/accounts/{id}/movements` is requested  
Then each returned row's `RunningBalance` reflects the ledger balance immediately after that movement in chronological order for the full filtered range  
And running-balance correctness does not depend on how many rows are visible in the current page

#### Scenario: Historical movement browsing is read-only
Given an authenticated user opens a historical movements route for a selected year/account  
When movement rows are shown  
Then running balance is displayed for each row  
And mutation actions (create/edit/delete) are not available from that historical view
