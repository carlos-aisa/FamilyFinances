## MODIFIED Requirements

### Requirement: Accounts SHALL Support Full Lifecycle Operations
The accounts lifecycle MUST preserve existing create/read/update behavior while moving kind identity semantics from enum-based values to catalog-backed references.

#### Scenario: Account creation returns account DTO
Given a valid `CreateAccountRequest` with catalog-backed kind identity  
When the client calls `POST /api/v{version:apiVersion}/accounts`  
Then the API returns `200 OK`  
And the response includes account kind identity and display values derived from the kind catalog

#### Scenario: Rename of non-existing account returns not found
Given an account id that does not exist  
When the client calls `PATCH /api/v{version:apiVersion}/accounts/{id}/rename`  
Then the API returns `404 NotFound`

#### Scenario: Account creation rejects unknown kind identity
Given a `CreateAccountRequest` referencing a non-existing or inactive catalog kind  
When the client calls `POST /api/v{version:apiVersion}/accounts`  
Then the API returns `400 BadRequest`  
And the error contract describes invalid account kind selection

#### Scenario: Account creation rejects kind identity incompatible with account nature
Given a `CreateAccountRequest` where selected catalog kind `Nature` does not match account `Nature`  
When the client calls `POST /api/v{version:apiVersion}/accounts`  
Then the API returns `400 BadRequest`  
And the error contract describes invalid account kind selection

#### Scenario: Existing account kind can be updated with compatibility checks
Given an existing account id and an active catalog kind compatible with that account `Nature`  
When the client calls `PATCH /api/v{version:apiVersion}/accounts/{id}/kind`  
Then the API returns `204 NoContent`  
And subsequent account reads expose the updated catalog kind identity
