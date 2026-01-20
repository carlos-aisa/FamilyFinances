using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFinances.Application.Reporting.Dtos;

public sealed record AccountMovementDto(
    Guid TransactionId,
    DateOnly BookedOn,
    string Description,
    string? PayeeName,
    decimal SignedAmount,
    string? CounterpartyAccountName
);

public sealed record AccountMovementsDto(
    Guid AccountId,
    string AccountName,
    DateOnly FromInclusive,
    DateOnly ToExclusive,
    IReadOnlyList<AccountMovementDto> Items,
    int TotalCount
);
