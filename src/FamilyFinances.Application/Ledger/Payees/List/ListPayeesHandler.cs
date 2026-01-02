using FamilyFinances.Application.Abstractions;
using FamilyFinances.Domain.Ledger.Payees;

namespace FamilyFinances.Application.Ledger.Payees.List;

public sealed class ListPayeesHandler
{
    private readonly IPayeeRepository _payees;

    public ListPayeesHandler(IPayeeRepository payees)
    {
        _payees = payees;
    }

    public Task<IReadOnlyList<Payee>> HandleAsync(ListPayeesQuery _, CancellationToken ct)
        => _payees.ListAsync(ct);
}
