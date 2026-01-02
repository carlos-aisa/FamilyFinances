using FamilyFinances.Domain.Common;

namespace FamilyFinances.Domain.Tests.Common;

public sealed class MoneyTests
{
    [Fact]
    public void Zero_IsZero()
    {
        var m = Money.Zero;
        Assert.True(m.IsZero);
        Assert.Equal(0, m.Cents);
    }

    [Fact]
    public void Addition_Works()
    {
        var a = new Money(150); // 1.50
        var b = new Money(250); // 2.50

        var c = a + b;

        Assert.Equal(400, c.Cents);
        Assert.Equal(4.00m, c.ToEuros());
    }

    [Fact]
    public void Subtraction_Works()
    {
        var a = new Money(500); // 5.00
        var b = new Money(125); // 1.25

        var c = a - b;

        Assert.Equal(375, c.Cents);
        Assert.Equal(3.75m, c.ToEuros());
    }

    [Fact]
    public void Negation_Works()
    {
        var a = new Money(123);
        var b = -a;

        Assert.Equal(-123, b.Cents);
    }

    [Fact]
    public void FromEuros_Rounds_AwayFromZero()
    {
        // 1.005 -> 1.01 (100.5 cents -> 101)
        var m = Money.FromEuros(1.005m);
        Assert.Equal(101, m.Cents);

        // -1.005 -> -1.01
        var n = Money.FromEuros(-1.005m);
        Assert.Equal(-101, n.Cents);
    }

    [Fact]
    public void Checked_Arithmetic_Throws_OnOverflow()
    {
        var a = new Money(long.MaxValue);
        Assert.Throws<OverflowException>(() => _ = a + new Money(1));
    }
}
