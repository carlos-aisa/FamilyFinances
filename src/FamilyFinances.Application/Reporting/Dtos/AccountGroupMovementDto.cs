using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Dtos;

public sealed record AccountGroupMovementDto(
    Guid TransactionId,
    DateOnly BookedOn,
    string Description,
    string? PayeeName,
    IReadOnlyList<string> SourceAccountNames,
    IReadOnlyList<string> DestinationAccountNames,
    long GroupNetCents,
    long RunningNetCents);

public sealed record AccountGroupMovementsDto(
    Guid GroupId,
    string GroupName,
    DateOnly FromInclusive,
    DateOnly ToExclusive,
    AccountNature? Nature,
    IReadOnlyList<AccountGroupMovementDto> Items);
