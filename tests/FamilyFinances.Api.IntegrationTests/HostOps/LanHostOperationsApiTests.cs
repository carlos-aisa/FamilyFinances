using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FamilyFinances.Api.Features.HostOps;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilyFinances.Api.IntegrationTests.HostOps;

public sealed class LanHostOperationsApiTests
{
    [Fact]
    public async Task Status_Without_Token_Returns_Unauthorized()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"ff-hostops-{Guid.NewGuid():N}.db");
        await using var factory = CreateFactory(dbPath, new FakeLanHostOperationsService());
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions());

        var response = await client.GetAsync("/api/v1/ops/lan/status");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Apply_And_Status_With_Admin_Token_Updates_State()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"ff-hostops-{Guid.NewGuid():N}.db");
        var fake = new FakeLanHostOperationsService();
        await using var factory = CreateFactory(dbPath, fake);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions());

        var token = await TestAuth.LoginAndGetTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var applyResponse = await client.PostAsJsonAsync(
            "/api/v1/ops/lan/apply",
            new LanAccessRequest(Enabled: true, HttpsPort: 5443, HostName: "familyfinances.local", RegenerateCertificate: false));

        applyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var applyResult = await applyResponse.Content.ReadFromJsonAsync<LanOperationResult>();
        applyResult.Should().NotBeNull();
        applyResult!.Succeeded.Should().BeTrue();

        var statusResponse = await client.GetAsync("/api/v1/ops/lan/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var status = await statusResponse.Content.ReadFromJsonAsync<LanAccessStatus>();
        status.Should().NotBeNull();
        status!.Enabled.Should().BeTrue();
        status.HttpsPort.Should().Be(5443);
        status.HostName.Should().Be("familyfinances.local");
    }

    private static WebApplicationFactory<Program> CreateFactory(string dbPath, ILanHostOperationsService hostOps)
    {
        return new CustomWebApplicationFactory(dbPath)
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ILanHostOperationsService>();
                    services.AddSingleton(hostOps);
                });
            });
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
            FirewallEnabled: false,
            AccessLimited: false,
            Diagnostic: null);

        public Task<LanAccessStatus> GetStatusAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_status);
        }

        public Task<LanOperationResult> ApplyAsync(LanAccessRequest request, string actor, CancellationToken ct = default)
        {
            _status = _status with
            {
                Enabled = request.Enabled,
                HttpsPort = request.HttpsPort,
                HostName = string.IsNullOrWhiteSpace(request.HostName) ? Environment.MachineName : request.HostName,
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
                HostName = string.IsNullOrWhiteSpace(hostName) ? Environment.MachineName : hostName,
                CertificateThumb = "THUMB-ROTATED",
                CertificateSubject = "CN=familyfinances.local",
                FirewallEnabled = true
            };

            return Task.FromResult(new LanOperationResult(true, "ok", _status));
        }
    }
}
