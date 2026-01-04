using FamilyFinances.Domain.Ledger.Payees;

namespace FamilyFinances.Application.Ledger.Payees.Dtos;

public sealed record PayeeDto(Guid Id, string Name);