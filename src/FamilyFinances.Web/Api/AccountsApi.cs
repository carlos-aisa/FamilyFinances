using FamilyFinances.Application.Ledger.Accounts.Dtos;

namespace FamilyFinances.Web.Api
{
    public sealed class AccountsApi
    {
        private readonly HttpClient _http;

        public AccountsApi(HttpClient http) => _http = http;

        public async Task<IReadOnlyList<AccountDto>> ListAsync(CancellationToken ct)
        {
            var items = await _http.GetFromJsonAsync<IReadOnlyList<AccountDto>>("api/v1/accounts", ct);
            return items ?? Array.Empty<AccountDto>();
        }
    }
}
