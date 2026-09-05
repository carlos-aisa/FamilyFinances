using FamilyFinances.Domain.Ledger.AccountGroups;

namespace FamilyFinances.Domain.Tests.Ledger.AccountGroups;

public sealed class AccountGroupTests
{
    [Fact]
    public void Create_AssignsId_AndSetsFields()
    {
        var group = AccountGroup.Create("Home Expenses", "All household related expenses");

        Assert.NotEqual(default, group.Id.Value);
        Assert.Equal("Home Expenses", group.Name);
        Assert.Equal("All household related expenses", group.Description);
        Assert.Equal("HOME EXPENSES", group.NormalizedName);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var group = AccountGroup.Create("  Carlos  ", "desc");

        Assert.Equal("Carlos", group.Name);
        Assert.Equal("CARLOS", group.NormalizedName);
    }

    [Fact]
    public void Create_AllowsNullDescription()
    {
        var group = AccountGroup.Create("Group", null);

        Assert.Equal("Group", group.Name);
        Assert.Null(group.Description);
    }

    [Fact]
    public void DashboardPin_DefaultsToFalse_AndCanBeUpdated()
    {
        var group = AccountGroup.Create("Group", null);

        Assert.False(group.IsDashboardPinned);

        group.SetDashboardPinned(true);
        Assert.True(group.IsDashboardPinned);

        group.SetDashboardPinned(false);
        Assert.False(group.IsDashboardPinned);
    }

    [Fact]
    public void Create_TrimsAndNormalizesDescription()
    {
        var group = AccountGroup.Create("Group", "   ");

        Assert.Null(group.Description);
    }

    [Fact]
    public void Create_ShouldThrow_WhenNameIsNull()
    {
        var act = () => AccountGroup.Create(null!, "desc");

        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenNameIsEmptyOrWhitespace(string name)
    {
        var act = () => AccountGroup.Create(name, "desc");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Rename_UpdatesNameAndNormalizedName()
    {
        var group = AccountGroup.Create("Old Name", null);

        group.Rename("New Name");

        Assert.Equal("New Name", group.Name);
        Assert.Equal("NEW NAME", group.NormalizedName);
    }

    [Fact]
    public void Rename_TrimsName()
    {
        var group = AccountGroup.Create("Old", null);

        group.Rename("  New  ");

        Assert.Equal("New", group.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_ShouldThrow_WhenNameIsInvalid(string newName)
    {
        var group = AccountGroup.Create("Valid", null);

        var act = () => group.Rename(newName);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void UpdateDescription_UpdatesDescription()
    {
        var group = AccountGroup.Create("Group", "Old desc");

        group.UpdateDescription("New description");

        Assert.Equal("New description", group.Description);
    }

    [Fact]
    public void UpdateDescription_AllowsNull()
    {
        var group = AccountGroup.Create("Group", "Has description");

        group.UpdateDescription(null);

        Assert.Null(group.Description);
    }

    [Fact]
    public void UpdateDescription_TreatsWhitespaceAsNull()
    {
        var group = AccountGroup.Create("Group", "Has description");

        group.UpdateDescription("   ");

        Assert.Null(group.Description);
    }

    [Fact]
    public void UpdateDescription_TrimsValue()
    {
        var group = AccountGroup.Create("Group", null);

        group.UpdateDescription("  trimmed  ");

        Assert.Equal("trimmed", group.Description);
    }
}
