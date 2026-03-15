namespace FamilyFinances.Web.Features.HostOps;

public interface ILanHostOperationsService
{
    Task<LanAccessStatus> GetStatusAsync(CancellationToken ct = default);
    Task<LanOperationResult> ApplyAsync(LanAccessRequest request, string actor, CancellationToken ct = default);
    Task<LanOperationResult> RegenerateCertificateAsync(int httpsPort, string? hostName, string actor, CancellationToken ct = default);
}
