namespace FamilyFinances.Web.Auth
{
    public sealed class ApiTokenStore : IApiTokenStore
    {
        private string? _accessToken;

        public string? GetAccessToken() => _accessToken;
        public void SetAccessToken(string accessToken) => _accessToken = accessToken;
        public void Clear() => _accessToken = null;
    }
}
