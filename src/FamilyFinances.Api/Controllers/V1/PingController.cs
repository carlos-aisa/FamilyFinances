using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFinances.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/ping")]
public sealed class PingController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "CanRead")]
    public IActionResult Get() => Ok(new { status = "ok" });
}
