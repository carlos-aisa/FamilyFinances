using System.Net;
using System.Net.Http.Json;
using Bunit;
using FamilyFinances.Application.Ledger.Payees.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Payees;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Payees;

public sealed class PayeesPageTests : WebTestContext
{
    [Fact]
    public void Payees_Render_As_Card_Grid_And_Search_Filters_Visible_Cards()
    {
        RegisterServices(
        [
            new PayeeDto(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Alice"),
            new PayeeDto(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Bob")
        ]);

        var cut = RenderComponent<PayeesPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Find(".payees-grid");
            cut.FindAll(".payee-card").Should().HaveCount(2);
        });

        cut.Find("input[placeholder='Search payees...']").Input("ali");

        cut.WaitForAssertion(() =>
        {
            var cards = cut.FindAll(".payee-card");
            cards.Should().HaveCount(1);
            cards[0].TextContent.Should().Contain("Alice");
        });
    }

    [Fact]
    public void Payees_Card_Actions_Support_Rename_And_Delete()
    {
        JSInterop.Setup<bool>("confirm", _ => true).SetResult(true);

        RegisterServices(
        [
            new PayeeDto(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Gym"),
            new PayeeDto(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Market")
        ]);

        var cut = RenderComponent<PayeesPage>();

        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".payee-card").Should().HaveCount(2);
        });

        cut.FindAll(".payee-card .btn-outline-primary")[0].Click();
        cut.WaitForAssertion(() =>
        {
            cut.Find(".payee-card input.form-control-sm");
        });

        cut.Find(".payee-card input.form-control-sm").Change("Gym Plus");
        cut.Find(".payee-card .btn-success").Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Gym Plus");
            cut.FindAll(".payee-card").Should().HaveCount(2);
        });

        cut.FindAll(".payee-card .btn-outline-danger")[1].Click();

        cut.WaitForAssertion(() =>
        {
            var cards = cut.FindAll(".payee-card");
            cards.Should().HaveCount(1);
            cards[0].TextContent.Should().Contain("Gym Plus");
            cards[0].TextContent.Should().NotContain("Market");
        });
    }

    private void RegisterServices(IReadOnlyList<PayeeDto> initialPayees)
    {
        var payees = initialPayees.ToList();

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(async (HttpRequestMessage req, CancellationToken ct) =>
            {
                var uri = req.RequestUri!.ToString();

                if (req.Method == HttpMethod.Get && uri.Contains("api/v1/payees", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(payees.ToArray())
                    };
                }

                if (req.Method.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase) &&
                    uri.Contains("api/v1/payees/", StringComparison.OrdinalIgnoreCase) &&
                    uri.Contains("/rename", StringComparison.OrdinalIgnoreCase))
                {
                    var payload = await req.Content!.ReadFromJsonAsync<RenamePayload>(cancellationToken: ct);
                    var payeeIdText = uri.Split('/').Reverse().Skip(1).First();
                    var payeeId = Guid.Parse(payeeIdText);
                    var index = payees.FindIndex(p => p.Id == payeeId);
                    if (index >= 0 && payload is not null)
                    {
                        payees[index] = payees[index] with { Name = payload.Name };
                    }

                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                }

                if (req.Method == HttpMethod.Delete && uri.Contains("api/v1/payees/", StringComparison.OrdinalIgnoreCase))
                {
                    var payeeIdText = uri.Split('/').Last();
                    var payeeId = Guid.Parse(payeeIdText);
                    payees.RemoveAll(p => p.Id == payeeId);

                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        var factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factoryMock
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var tokenStore = new TestTokenStore("test-token");
        var authProvider = new JwtAuthStateProvider(tokenStore);

        Services.AddSingleton(factoryMock.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(authProvider);
        Services.AddScoped<PayeesApi>();
    }

    private sealed record RenamePayload(string Name);

    private sealed class TestTokenStore : IApiTokenStore
    {
        private string? _token;

        public TestTokenStore(string? token)
        {
            _token = token;
        }

        public string? GetAccessToken() => _token;

        public void SetAccessToken(string accessToken) => _token = accessToken;

        public void Clear() => _token = null;

        public Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct) => Task.FromResult(_token);
    }
}
