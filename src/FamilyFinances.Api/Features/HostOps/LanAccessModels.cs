namespace FamilyFinances.Api.Features.HostOps;

public sealed record LanAccessRequest(
    bool Enabled,
    int HttpsPort = 5443,
    string? HostName = null,
    bool RegenerateCertificate = false);

public sealed record LanAccessStatus(
    bool Enabled,
    int HttpsPort,
    string? HostName,
    string? CertificateThumb,
    string? CertificateSubject,
    string FirewallRuleName,
    bool FirewallEnabled,
    bool AccessLimited = false,
    string? Diagnostic = null);

public sealed record LanOperationResult(
    bool Succeeded,
    string Message,
    LanAccessStatus? Status = null);
