using FamilyFinances.Application.Ledger.Payees.Abstractions;
using FamilyFinances.Application.Ledger.Payees.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Payees;

namespace FamilyFinances.Application.Ledger.Payees.Handlers;

public sealed class CreatePayeeHandler
{
    private readonly IPayeeRepository _payees;
    private readonly ILedgerUnitOfWork _uow;

    public CreatePayeeHandler(IPayeeRepository payees, ILedgerUnitOfWork uow)
    {
        _payees = payees;
        _uow = uow;
    }

    public async Task<PayeeId> HandleAsync(CreatePayeeRequest command, CancellationToken ct)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        var normalizedName = NameNormalizer.Normalize(command.Name);

        var existing = await _payees.GetByNormalizedNameAsync(normalizedName, ct);
        if (existing is not null)
            throw new ConflictException($"Payee '{command.Name}' already exists.");

        var payee = Payee.Create(command.Name);

        await _payees.AddAsync(payee, ct);
        await _uow.SaveChangesAsync(ct);

        return payee.Id;
    }
}
