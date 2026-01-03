using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Application.Ledger.AccountGroups.Dtos;
using FamilyFinances.Application.Ledger.AccountGroups.Requests;
using FamilyFinances.Domain.Ledger.AccountGroups;

namespace FamilyFinances.Application.Ledger.AccountGroups.Handlers
{
    public sealed class CreateAccountGroupHandler
    {
        private readonly IAccountGroupRepository _groups;

        public CreateAccountGroupHandler(IAccountGroupRepository groups)
        {
            _groups = groups;
        }

        public async Task<AccountGroupDto> HandleAsync(
            CreateAccountGroupRequest request,
            CancellationToken ct)
        {
            var normalized = request.Name.Trim().ToUpperInvariant();

            var existing = await _groups.GetByNormalizedNameAsync(normalized, ct);
            if (existing is not null)
                throw new InvalidOperationException("An account group with the same name already exists.");

            var group = AccountGroup.Create(
                request.Name.Trim(),
                request.Description?.Trim());

            await _groups.AddAsync(group, ct);

            return new AccountGroupDto(
                group.Id.Value,
                group.Name,
                group.Description);
        }
    }
}
