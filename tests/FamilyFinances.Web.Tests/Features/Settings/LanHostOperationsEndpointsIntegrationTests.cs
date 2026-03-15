using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FamilyFinances.Web.Endpoints;
using FamilyFinances.Web.Features.HostOps;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyFinances.Web.Tests.Features.Settings;

public sealed class LanHostOperationsEndpointsIntegrationTests
{
    [Fact]
    public async Task Status_WithoutAuthCookie_ReturnsUnauthorized()
    {
        await using var app = await CreateTestAppAsync(new FakeLanHostOperationsService());
        var client = app.GetTestClient();

        var response = await client.GetAsync("/ops/lan/status");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Apply_Then_Status_WithAdminCookie_TransitionsState()
    {
        var fakeOps = new FakeLanHostOperationsService();
        await using var app = await CreateTestAppAsync(fakeOps);
        var client = app.GetTestClient();

        var adminToken = CreateUnsignedJwt(new Dictionary<string, object>
        {
            ["sub"] = "admin-id",
            ["email"] = "admin@familyfinances.local",
            ["role"] = "Admin"
        });

        client.DefaultRequestHeaders.Add("Cookie", $"ff_access_token={adminToken}");

        var applyResponse = await client.PostAsJsonAsync(
            "/ops/lan/apply",
            new LanAccessRequest(Enabled: true, HttpsPort: 5443, HostName: "familyfinances.local", RegenerateCertificate: false));

        applyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var statusResponse = await client.GetAsync("/ops/lan/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var status = await statusResponse.Content.ReadFromJsonAsync<LanAccessStatus>();
        status.Should().NotBeNull();
        status!.Enabled.Should().BeTrue();
        status.HttpsPort.Should().Be(5443);
        status.HostName.Should().Be("familyfinances.local");
    }

    private static async Task<WebApplication> CreateTestAppAsync(ILanHostOperationsService service)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(service);

        var app = builder.Build();
        app.MapLanHostOperationsEndpoints();
        await app.StartAsync();
        return app;
    }

    private static string CreateUnsignedJwt(IReadOnlyDictionary<string, object> payload)
    {
        var header = new Dictionary<string, object>
        {
            ["alg"] = "none",
            ["typ"] = "JWT"
        };

        return $"{Base64Url(header)}.{Base64Url(payload)}.signature";
    }

    private static string Base64Url(IReadOnlyDictionary<string, object> part)
    {
        var json = JsonSerializer.Serialize(part);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private sealed class FakeLanHostOperationsService : ILanHostOperationsService
    {
        private LanAccessStatus _status = new(
            Enabled: false,
            HttpsPort: 5443,
            HostName: Environment.MachineName,
            CertificateThumb: null,
            CertificateSubject: null,
            FirewallRuleName: "FamilyFinances.Web.LAN.HTTPS",
            FirewallEnabled: false);

        public Task<LanAccessStatus> GetStatusAsync(CancellationToken ct = default) => Task.FromResult(_status);

        public Task<LanOperationResult> ApplyAsync(LanAccessRequest request, string actor, CancellationToken ct = default)
        {
            _status = _status with
            {
                Enabled = request.Enabled,
                HttpsPort = request.HttpsPort,
                HostName = request.HostName ?? Environment.MachineName,
                CertificateThumb = request.RegenerateCertificate ? "THUMB-ROTATED" : "THUMB-INITIAL",
                CertificateSubject = "CN=familyfinances.local",
                FirewallEnabled = request.Enabled
            };

            return Task.FromResult(new LanOperationResult(true, "ok", _status));
        }

        public Task<LanOperationResult> RegenerateCertificateAsync(int httpsPort, string? hostName, string actor, CancellationToken ct = default)
        {
            _status = _status with
            {
                Enabled = true,
                HttpsPort = httpsPort,
                HostName = hostName ?? Environment.MachineName,
                CertificateThumb = "THUMB-ROTATED",
                CertificateSubject = "CN=familyfinances.local",
                FirewallEnabled = true
            };

            return Task.FromResult(new LanOperationResult(true, "ok", _status));
        }
    }
}
