using System.Security.Claims;
using Asp.Versioning;
using FamilyFinances.Api.Features.HostOps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FamilyFinances.Infrastructure.Identity.AuthConstants;

namespace FamilyFinances.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/ops/lan")]
[Authorize(Policy = Policies.CanWrite)]
public sealed class LanHostOperationsController : ControllerBase
{
    [HttpGet("status")]
    public async Task<ActionResult<LanAccessStatus>> GetStatus(
        [FromServices] ILanHostOperationsService hostOps,
        CancellationToken ct)
    {
        var status = await hostOps.GetStatusAsync(ct);
        return Ok(status);
    }

    [HttpPost("apply")]
    public async Task<ActionResult<LanOperationResult>> Apply(
        [FromBody] LanAccessRequest request,
        [FromServices] ILanHostOperationsService hostOps,
        CancellationToken ct)
    {
        var actor = ResolveActor();
        var result = await hostOps.ApplyAsync(request, actor, ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("certificate/regenerate")]
    public async Task<ActionResult<LanOperationResult>> RegenerateCertificate(
        [FromBody] LanAccessRequest request,
        [FromServices] ILanHostOperationsService hostOps,
        CancellationToken ct)
    {
        if (!LanAccessCommandValidator.IsValidPort(request.HttpsPort))
        {
            return BadRequest(new LanOperationResult(false, $"Invalid HTTPS port {request.HttpsPort}."));
        }

        var actor = ResolveActor();
        var result = await hostOps.RegenerateCertificateAsync(request.HttpsPort, request.HostName, actor, ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    private string ResolveActor()
    {
        var email = User.Claims.FirstOrDefault(c =>
            string.Equals(c.Type, ClaimTypes.Email, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Type, "email", StringComparison.OrdinalIgnoreCase));

        var subject = User.Claims.FirstOrDefault(c =>
            string.Equals(c.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Type, "sub", StringComparison.OrdinalIgnoreCase));

        return email?.Value ?? subject?.Value ?? "unknown";
    }
}
