using FamilyFinances.Domain.Common;

namespace FamilyFinances.Domain.Common;

/// <summary>
/// Represents money using minor units (cents) to avoid floating point errors.
/// Single-currency ledger (EUR).
/// </summary>
public readonly record struct Money(long Cents)
{
     public static Currency Currency => Currency.EUR;

    public static Money Zero => new(0);

    public bool IsZero => Cents == 0;

    public static Money FromEuros(decimal euros)
    {
        // Accept decimal as an input convenience (e.g., API/UI),
        // but store internally as integer minor units.
        var cents = checked((long)Math.Round(euros * 100m, MidpointRounding.AwayFromZero));
        return new Money(cents);
    }

    public decimal ToEuros() => Cents / 100m;

    // Use checked to ensure that an overflow throws an exception explicitly
    public static Money operator +(Money a, Money b) => new(checked(a.Cents + b.Cents));
    public static Money operator -(Money a, Money b) => new(checked(a.Cents - b.Cents));
    public static Money operator -(Money a) => new(checked(-a.Cents));

    public override string ToString() => $"{ToEuros():0.00} {Currency}";

    public void EnsureNotOverflowSafe()
    {
        //TODO: implement checks for operations that may overflow
    }
}
