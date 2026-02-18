## MODIFIED Requirements

### Requirement: Account Movements View Must Display Running Balance Evolution
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

#### Scenario: Historical movement browsing is read-only
Given an authenticated user opens a historical movements route for a selected year/account  
When movement rows are shown  
Then running balance is displayed for each row  
And mutation actions (create/edit/delete) are not available from that historical view

### Requirement: Account Reconciliation Must Return Reconciliation Result
#### Scenario: Reconcile existing account
Given an existing account id and valid `ReconcileAccountRequest`  
When the client calls `POST /api/v{version:apiVersion}/accounts/{id}/reconcile`  
Then the API returns `200 OK`  
And the response is `ReconcileAccountResponse { AdjustmentCreated, TransactionId, ComputedBalance, ActualBalance, Difference, Message }`

#### Scenario: Reconcile request in closed fiscal year is rejected
Given a valid account id and `ReconcileAccountRequest` where `AsOfDate.Year` is closed  
When the client calls `POST /api/v{version:apiVersion}/accounts/{id}/reconcile`  
Then the API returns `400 BadRequest`  
And the response body uses `{ error }` describing the closed-year restriction

### Requirement: Transactions Must Enforce Balanced Splits and Support Query/Mutation Endpoints
#### Scenario: Unbalanced transaction creation is rejected
Given a `CreateTransactionRequest` where split cents do not sum to zero  
When the client calls `POST /api/v{version:apiVersion}/transactions`  
Then the API returns `400 BadRequest`  
And the response uses `{ error }` (domain exception mapping)

#### Scenario: Existing transaction can be deleted
Given a transaction id that exists  
When the client calls `DELETE /api/v{version:apiVersion}/transactions/{id}`  
Then the API returns `204 NoContent`

#### Scenario: Closed-year transaction creation is rejected
Given a `CreateTransactionRequest` where `BookedOn.Year` is closed  
When the client calls `POST /api/v{version:apiVersion}/transactions`  
Then the API returns `400 BadRequest`  
And the response uses `{ error }` describing the closed-year restriction

#### Scenario: Closed-year transaction update is rejected
Given an update request targeting a transaction booked in closed year `Y`  
When the client calls `PUT /api/v{version:apiVersion}/transactions/{id}` or `PUT /api/v{version:apiVersion}/transactions/{id}/multi-split`  
Then the API returns `400 BadRequest`  
And the response uses `{ error }` describing the closed-year restriction

#### Scenario: Closed-year transaction deletion is rejected
Given a transaction id whose stored `BookedOn.Year` is closed  
When the client calls `DELETE /api/v{version:apiVersion}/transactions/{id}`  
Then the API returns `400 BadRequest`  
And the response uses `{ error }` describing the closed-year restriction
