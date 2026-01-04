using FamilyFinances.Web.Auth;
using System.Net.Http.Headers;

namespace FamilyFinances.Web.Api
{
    public sealed class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IApiTokenStore _tokenStore;

        public AuthHeaderHandler(IApiTokenStore tokenStore)
            => _tokenStore = tokenStore;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _tokenStore.GetAccessToken();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
