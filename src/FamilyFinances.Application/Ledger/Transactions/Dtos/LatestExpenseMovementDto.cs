namespace FamilyFinances.Application.Ledger.Transactions.Dtos;

public sealed record LatestExpenseMovementDto(
    Guid Id,
    DateOnly BookedOn,
    string? Description,
    long AmountCents);
