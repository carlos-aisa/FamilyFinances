using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Payees;

namespace FamilyFinances.Domain.Tests.Ledger.Payees;

public sealed class PayeeTests
{
    [Fact]
    public void Create_ShouldThrow_WhenNameIsNull()
    {
        var act = () => Payee.Create(null!);

        Assert.Throws<DomainException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenNameIsEmptyOrWhitespace(string name)
    {
        var act = () => Payee.Create(name);

        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        var name = "   Netflix   ";

        var payee = Payee.Create(name);

        Assert.Equal("Netflix", payee.Name);
    }

    [Fact]
    public void Create_ShouldSetNormalizedName_ToUpperInvariant()
    {
        var name = "Netflix España";

        var payee = Payee.Create(name);

        Assert.Equal("NETFLIX ESPAÑA", payee.NormalizedName);
    }

    [Fact]
    public void Create_ShouldGenerateNewPayeeId()
    {
        var payee = Payee.Create("Netflix");

        Assert.NotEqual(Guid.Empty, payee.Id.Value);
    }

    [Fact]
    public void Rename_ShouldUpdateNameAndNormalizedName()
    {
        var payee = Payee.Create("Netflix");

        payee.Rename("Spotify");

        Assert.Equal("Spotify", payee.Name);
        Assert.Equal("SPOTIFY", payee.NormalizedName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_ShouldThrow_WhenNameIsInvalid(string newName)
    {
        var payee = Payee.Create("Netflix");

        var act = () => payee.Rename(newName);

        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void SetDefaultCategory_ShouldStoreNull_WhenWhitespace()
    {
        var payee = Payee.Create("Netflix");

        payee.SetDefaultCategory("   ");

        Assert.Null(payee.DefaultCategory);
    }

    [Fact]
    public void SetDefaultCategory_ShouldTrimValue()
    {
        var payee = Payee.Create("Netflix");

        payee.SetDefaultCategory("  Subscriptions  ");

        Assert.Equal("Subscriptions", payee.DefaultCategory);
    }

    [Fact]
    public void Create_WithDefaultCategory_ShouldSetCategory()
    {
        var payee = Payee.Create("Netflix", "Entertainment");

        Assert.Equal("Entertainment", payee.DefaultCategory);
    }

    [Fact]
    public void Create_ShouldThrow_WhenNameTooLong()
    {
        var longName = new string('a', 201);

        var act = () => Payee.Create(longName);

        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Create_ShouldThrow_WhenDefaultCategoryTooLong()
    {
        var longCategory = new string('a', 101);

        var act = () => Payee.Create("Netflix", longCategory);

        Assert.Throws<DomainException>(act);
    }
}
