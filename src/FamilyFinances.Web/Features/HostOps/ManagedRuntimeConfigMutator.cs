using System.Text.Json.Nodes;

namespace FamilyFinances.Web.Features.HostOps;

public static class ManagedRuntimeConfigMutator
{
    public static JsonObject ApplyApiProductionOverrides(
        JsonObject apiConfig,
        string runtimeRoot,
        int apiPort,
        string jwtKey)
    {
        var connectionStrings = apiConfig["ConnectionStrings"] as JsonObject ?? new JsonObject();
        connectionStrings["Default"] = $"Data Source={runtimeRoot}\\data\\familyfinances.db";
        apiConfig["ConnectionStrings"] = connectionStrings;

        var jwt = apiConfig["Jwt"] as JsonObject ?? new JsonObject();
        jwt["Key"] = jwtKey;
        apiConfig["Jwt"] = jwt;

        var kestrel = apiConfig["Kestrel"] as JsonObject ?? new JsonObject();
        var endpoints = kestrel["Endpoints"] as JsonObject ?? new JsonObject();
        var http = endpoints["Http"] as JsonObject ?? new JsonObject();
        http["Url"] = $"http://127.0.0.1:{apiPort}";
        endpoints["Http"] = http;
        kestrel["Endpoints"] = endpoints;
        apiConfig["Kestrel"] = kestrel;

        return apiConfig;
    }

    public static JsonObject ApplyWebProductionOverrides(
        JsonObject webConfig,
        int apiPort,
        int webPort)
    {
        var api = webConfig["Api"] as JsonObject ?? new JsonObject();
        api["BaseUrl"] = $"http://127.0.0.1:{apiPort}/";
        webConfig["Api"] = api;

        var kestrel = webConfig["Kestrel"] as JsonObject ?? new JsonObject();
        var endpoints = kestrel["Endpoints"] as JsonObject ?? new JsonObject();
        var http = endpoints["Http"] as JsonObject ?? new JsonObject();
        http["Url"] = $"http://127.0.0.1:{webPort}";
        endpoints["Http"] = http;
        kestrel["Endpoints"] = endpoints;
        webConfig["Kestrel"] = kestrel;

        return webConfig;
    }
}
