using FamilyFinances.Application.Ledger.Payees.Abstractions;
using FamilyFinances.Application.Ledger.Payees.Requests;
using FamilyFinances.Domain.Ledger.Payees;

namespace FamilyFinances.Application.Ledger.Payees.Handlers;

public sealed class ListPayeesHandler
{
    private readonly IPayeeRepository _payees;

    public ListPayeesHandler(IPayeeRepository payees)
    {
        _payees = payees;
    }

    public Task<IReadOnlyList<Payee>> HandleAsync(ListPayeesRequest _, CancellationToken ct)
        => _payees.ListAsync(ct);
}
