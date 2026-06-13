using System.Globalization;
using System.Text;
using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Handlers;

public sealed class CreateAccountKindHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ILedgerUnitOfWork _uow;

    public CreateAccountKindHandler(IAccountRepository accounts, ILedgerUnitOfWork uow)
    {
        _accounts = accounts;
        _uow = uow;
    }

    public async Task<AccountKindCatalogDto> HandleAsync(CreateAccountKindRequest request, CancellationToken ct)
    {
        var name = (request.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Account kind name is required.");

        var key = await BuildUniqueKeyAsync(name, ct);
        var existing = await _accounts.ListKindsAsync(includeInactive: true, ct);
        var nextSortOrder = existing.Count == 0 ? 1000 : existing.Max(x => x.SortOrder) + 10;

        var entity = AccountKindCatalog.CreateCustom(key, name, nextSortOrder, request.Nature);

        await _accounts.AddKindAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return new AccountKindCatalogDto(
            entity.Id.Value,
            entity.Key,
            entity.Name,
            entity.IsSystem,
            entity.IsActive,
            entity.SortOrder,
            entity.LegacyKind,
            entity.Nature);
    }

    private async Task<string> BuildUniqueKeyAsync(string name, CancellationToken ct)
    {
        var baseKey = Slugify(name);
        var candidate = baseKey;
        var suffix = 2;

        while (await _accounts.ExistsKindByKeyAsync(candidate, excludingId: null, ct))
        {
            candidate = $"{baseKey}-{suffix}";
            suffix++;
        }

        return candidate;
    }

    private static string Slugify(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
                continue;
            }

            if (ch == ' ' || ch == '-' || ch == '_')
                builder.Append('-');
        }

        var slug = builder.ToString().Trim('-');
        while (slug.Contains("--", StringComparison.Ordinal))
            slug = slug.Replace("--", "-", StringComparison.Ordinal);

        return string.IsNullOrWhiteSpace(slug) ? "custom-kind" : slug;
    }
}
