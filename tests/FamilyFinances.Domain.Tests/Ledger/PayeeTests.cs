using System;
using FamilyFinances.Domain.Ledger;
using FamilyFinances.Domain.Common;
using Xunit;

namespace FamilyFinances.Domain.Tests.Ledger.Payees;

public class PayeeTests
{
    [Fact]
    public void Create_ShouldThrow_WhenNameIsNull()
    {
        // Act
        var act = () => Payee.Create(null!);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenNameIsEmptyOrWhitespace(string name)
    {
        // Act
        var act = () => Payee.Create(name);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        // Arrange
        var name = "   Netflix   ";

        // Act
        var payee = Payee.Create(name);

        // Assert
        Assert.Equal("Netflix", payee.Name);
    }

    [Fact]
    public void Create_ShouldSetNormalizedName_ToUpperInvariant()
    {
        // Arrange
        var name = "Netflix España";

        // Act
        var payee = Payee.Create(name);

        // Assert
        Assert.Equal("NETFLIX ESPAÑA", payee.NormalizedName);
    }

    [Fact]
    public void Create_ShouldGenerateNewPayeeId()
    {
        // Act
        var payee = Payee.Create("Netflix");

        // Assert
        Assert.NotEqual(Guid.Empty, payee.Id.Value);
    }

    [Fact]
    public void Rename_ShouldUpdateNameAndNormalizedName()
    {
        // Arrange
        var payee = Payee.Create("Netflix");

        // Act
        payee.Rename("Spotify");

        // Assert
        Assert.Equal("Spotify", payee.Name);
        Assert.Equal("SPOTIFY", payee.NormalizedName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_ShouldThrow_WhenNameIsInvalid(string newName)
    {
        // Arrange
        var payee = Payee.Create("Netflix");

        // Act
        var act = () => payee.Rename(newName);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void SetDefaultCategory_ShouldStoreNull_WhenWhitespace()
    {
        // Arrange
        var payee = Payee.Create("Netflix");

        // Act
        payee.SetDefaultCategory("   ");

        // Assert
        Assert.Null(payee.DefaultCategory);
    }

    [Fact]
    public void SetDefaultCategory_ShouldTrimValue()
    {
        // Arrange
        var payee = Payee.Create("Netflix");

        // Act
        payee.SetDefaultCategory("  Subscriptions  ");

        // Assert
        Assert.Equal("Subscriptions", payee.DefaultCategory);
    }
}
