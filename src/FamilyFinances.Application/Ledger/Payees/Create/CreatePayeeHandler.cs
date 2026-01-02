using FamilyFinances.Application.Abstractions;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Payees;

namespace FamilyFinances.Application.Ledger.Payees.Create;

public sealed class CreatePayeeHandler
{
    private readonly IPayeeRepository _payees;
    private readonly ILedgerUnitOfWork _uow;

    public CreatePayeeHandler(IPayeeRepository payees, ILedgerUnitOfWork uow)
    {
        _payees = payees;
        _uow = uow;
    }

    public async Task<PayeeId> Handle(CreatePayeeCommand command, CancellationToken ct)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        var normalizedName = command.Name.Trim().ToUpperInvariant();

        var existing = await _payees.GetByNormalizedNameAsync(normalizedName, ct);
        if (existing is not null)
            throw new DomainException($"Payee '{command.Name}' already exists.");

        var payee = Payee.Create(command.Name);

        _payees.Add(payee);
        await _uow.SaveChangesAsync(ct);

        return payee.Id;
    }
}
