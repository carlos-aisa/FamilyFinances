namespace FamilyFinances.Web.Components.Shared;

public sealed record MovementListItem(
    DateOnly BookedOn,
    string? Description,
    long AmountCents);
