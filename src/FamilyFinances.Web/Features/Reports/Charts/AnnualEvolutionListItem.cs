namespace FamilyFinances.Web.Features.Reports.Charts;

public sealed record AnnualEvolutionListItem(
    Guid? EntityId,
    string Key,
    string Label,
    long? CurrentBalanceCents,
    long? MonthChangeCents,
    long? YearToDateCents,
    string SemanticClass
);

public enum AnnualEvolutionListLayout
{
    AccountGroup,
    Account
}
