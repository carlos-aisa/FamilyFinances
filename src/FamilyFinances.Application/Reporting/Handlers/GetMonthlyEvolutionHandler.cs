using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Common;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetMonthlyEvolutionHandler
{
    private readonly IReportingReadRepository _repo;

    public GetMonthlyEvolutionHandler(IReportingReadRepository repo)
    {
        _repo = repo;
    }

    public Task<MonthlyEvolutionReportDto> HandleAsync(GetMonthlyEvolutionQuery query, CancellationToken ct)
    {
        var currentYear = DateTime.UtcNow.Year;
        if (query.Year < 2000 || query.Year > currentYear)
            throw new DomainException($"Invalid year '{query.Year}'. Expected a value between 2000 and {currentYear}.");

        if (!Enum.IsDefined(query.Scope))
            throw new DomainException("Invalid scope.");

        return _repo.GetMonthlyEvolutionAsync(query.Year, query.Scope, ct);
    }
}
