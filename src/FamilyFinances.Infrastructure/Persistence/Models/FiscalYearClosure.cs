namespace FamilyFinances.Infrastructure.Persistence.Models;

public sealed class FiscalYearClosure
{
    public int Year { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public string? ClosedByUserId { get; set; }
    public DateTime? ReopenedAtUtc { get; set; }
    public string? ReopenedByUserId { get; set; }
}
