using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Application.Ledger.AccountGroups.Dtos;
using FamilyFinances.Application.Ledger.AccountGroups.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.AccountGroups;

namespace FamilyFinances.Application.Ledger.AccountGroups.Handlers
{
    public sealed class CreateAccountGroupHandler
    {
        private readonly IAccountGroupRepository _groups;
        private readonly ILedgerUnitOfWork _uow;

        public CreateAccountGroupHandler(IAccountGroupRepository groups, ILedgerUnitOfWork uow)
        {
            _groups = groups;
            _uow = uow;
        }

        public async Task<AccountGroupDto> HandleAsync(
            CreateAccountGroupRequest request,
            CancellationToken ct)
        {
            var normalized = NameNormalizer.Normalize(request.Name);

            var existing = await _groups.GetByNormalizedNameAsync(normalized, ct);
            if (existing is not null)
                throw new DomainException("An account group with the same name already exists.");

            var group = AccountGroup.Create(
                request.Name.Trim(),
                request.Description?.Trim());

            await _groups.AddAsync(group, ct);
            await _uow.SaveChangesAsync(ct);

            return new AccountGroupDto(
                group.Id.Value,
                group.Name,
                group.Description,
                group.IsDashboardPinned);
        }
    }
}
