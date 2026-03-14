using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Globalization;

namespace FamilyFinances.Web.Components.Pages.Transactions;

internal enum TransactionOrigin
{
    Transactions,
    AccountsMovements,
    HistoryTransactions,
    HistoryMovements,
    ReportCategoryTotals,
    ReportAccountTotals
}

internal sealed record TransactionOriginContext(
    TransactionOrigin Origin,
    Guid? AccountId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int? Year = null)
{
    public const string OriginQueryKey = "origin";
    public const string AccountIdQueryKey = "accountId";
    public const string FromQueryKey = "from";
    public const string ToQueryKey = "to";
    public const string YearQueryKey = "year";

    public static TransactionOriginContext FromNavigation(NavigationManager navigation)
        => FromUri(navigation.ToAbsoluteUri(navigation.Uri));

    public static TransactionOriginContext FromUri(Uri uri)
    {
        var query = QueryHelpers.ParseQuery(uri.Query);
        return FromQuery(query);
    }

    public static TransactionOriginContext FromQuery(IReadOnlyDictionary<string, StringValues> query)
    {
        return new TransactionOriginContext(
            Origin: ParseOrigin(ReadValue(query, OriginQueryKey)),
            AccountId: ParseGuid(ReadValue(query, AccountIdQueryKey)),
            From: ParseDateOnly(ReadValue(query, FromQueryKey)),
            To: ParseDateOnly(ReadValue(query, ToQueryKey)),
            Year: ParseInt(ReadValue(query, YearQueryKey)));
    }

    public static TransactionOriginContext FromQuery(IReadOnlyDictionary<string, string?> query)
    {
        return new TransactionOriginContext(
            Origin: ParseOrigin(ReadValue(query, OriginQueryKey)),
            AccountId: ParseGuid(ReadValue(query, AccountIdQueryKey)),
            From: ParseDateOnly(ReadValue(query, FromQueryKey)),
            To: ParseDateOnly(ReadValue(query, ToQueryKey)),
            Year: ParseInt(ReadValue(query, YearQueryKey)));
    }

    public Dictionary<string, string?> ToQuery(bool includeOrigin = true)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (includeOrigin)
            values[OriginQueryKey] = ToOriginToken(Origin);

        if (AccountId is not null)
            values[AccountIdQueryKey] = AccountId.Value.ToString();

        if (From is not null)
            values[FromQueryKey] = From.Value.ToString("yyyy-MM-dd");

        if (To is not null)
            values[ToQueryKey] = To.Value.ToString("yyyy-MM-dd");

        if (Year is not null)
            values[YearQueryKey] = Year.Value.ToString(CultureInfo.InvariantCulture);

        return values;
    }

    public string ToQueryString(bool includeOrigin = true)
    {
        var query = ToQuery(includeOrigin);
        if (query.Count == 0)
            return string.Empty;

        return QueryString.Create(query).ToUriComponent();
    }

    public string BuildUri(string basePath, bool includeOrigin = true)
    {
        var queryString = ToQueryString(includeOrigin);
        return string.IsNullOrWhiteSpace(queryString)
            ? basePath
            : $"{basePath}{queryString}";
    }

    private static TransactionOrigin ParseOrigin(string? raw)
    {
        return raw?.Trim().ToLowerInvariant() switch
        {
            "accounts-movements" => TransactionOrigin.AccountsMovements,
            "history-transactions" => TransactionOrigin.HistoryTransactions,
            "history-movements" => TransactionOrigin.HistoryMovements,
            "report-category-totals" => TransactionOrigin.ReportCategoryTotals,
            "report-account-totals" => TransactionOrigin.ReportAccountTotals,
            _ => TransactionOrigin.Transactions
        };
    }

    private static string ToOriginToken(TransactionOrigin origin)
    {
        return origin switch
        {
            TransactionOrigin.AccountsMovements => "accounts-movements",
            TransactionOrigin.HistoryTransactions => "history-transactions",
            TransactionOrigin.HistoryMovements => "history-movements",
            TransactionOrigin.ReportCategoryTotals => "report-category-totals",
            TransactionOrigin.ReportAccountTotals => "report-account-totals",
            _ => "transactions"
        };
    }

    private static string? ReadValue(IReadOnlyDictionary<string, StringValues> values, string key)
        => values.TryGetValue(key, out var raw) ? raw.ToString() : null;

    private static string? ReadValue(IReadOnlyDictionary<string, string?> values, string key)
        => values.TryGetValue(key, out var raw) ? raw : null;

    private static Guid? ParseGuid(string? raw)
        => Guid.TryParse(raw, out var parsed) ? parsed : null;

    private static DateOnly? ParseDateOnly(string? raw)
        => DateOnly.TryParse(raw, out var parsed) ? parsed : null;

    private static int? ParseInt(string? raw)
        => int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
}
