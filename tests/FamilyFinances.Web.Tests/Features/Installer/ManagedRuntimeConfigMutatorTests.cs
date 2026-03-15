using System.Text.Json.Nodes;
using FamilyFinances.Web.Features.HostOps;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Installer;

public sealed class ManagedRuntimeConfigMutatorTests
{
    [Fact]
    public void ApplyApiProductionOverrides_UpdatesLoopbackConnectionAndJwt()
    {
        var input = new JsonObject
        {
            ["ConnectionStrings"] = new JsonObject(),
            ["Jwt"] = new JsonObject(),
            ["Kestrel"] = new JsonObject
            {
                ["Endpoints"] = new JsonObject
                {
                    ["Http"] = new JsonObject()
                }
            }
        };

        var result = ManagedRuntimeConfigMutator.ApplyApiProductionOverrides(
            input,
            runtimeRoot: @"C:\ProgramData\FamilyFinances",
            apiPort: 5084,
            jwtKey: "generated-jwt-key-12345678901234567890");

        result["ConnectionStrings"]!["Default"]!.GetValue<string>()
            .Should().Be(@"Data Source=C:\ProgramData\FamilyFinances\data\familyfinances.db");
        result["Jwt"]!["Key"]!.GetValue<string>()
            .Should().Be("generated-jwt-key-12345678901234567890");
        result["Kestrel"]!["Endpoints"]!["Http"]!["Url"]!.GetValue<string>()
            .Should().Be("http://127.0.0.1:5084");
    }

    [Fact]
    public void ApplyWebProductionOverrides_UpdatesApiBaseUrlAndLoopbackBinding()
    {
        var input = new JsonObject
        {
            ["Api"] = new JsonObject(),
            ["Kestrel"] = new JsonObject
            {
                ["Endpoints"] = new JsonObject
                {
                    ["Http"] = new JsonObject()
                }
            }
        };

        var result = ManagedRuntimeConfigMutator.ApplyWebProductionOverrides(input, apiPort: 5084, webPort: 5019);

        result["Api"]!["BaseUrl"]!.GetValue<string>()
            .Should().Be("http://127.0.0.1:5084/");
        result["Kestrel"]!["Endpoints"]!["Http"]!["Url"]!.GetValue<string>()
            .Should().Be("http://127.0.0.1:5019");
    }
}
