using Bunit;
using FamilyFinances.Web.Components.Shared;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Components.Shared;

public sealed class MovementListTests : WebTestContext
{
    [Fact]
    public void RendersDateDescriptionAndPositiveNeutralAmount()
    {
        var cut = RenderComponent<MovementList>(parameters => parameters
            .Add(component => component.Items,
            [
                new MovementListItem(new DateOnly(2026, 3, 4), "Groceries", -5_412),
                new MovementListItem(new DateOnly(2026, 3, 3), null, 7_800)
            ])
            .Add(component => component.EmptyMessage, "No movements."));

        cut.FindAll("[data-testid='movement-list-item']").Should().HaveCount(2);
        cut.Markup.Should().Contain("Date").And.Contain("Description").And.Contain("Amount").And.Contain("Groceries");
        cut.FindAll("[data-testid='movement-list-amount']").Select(element => element.TextContent).Should().OnlyContain(value => !value!.Contains('-'));
        cut.FindAll("[data-testid='movement-list-amount']").Should().OnlyContain(element => !element.ClassList.Contains("text-danger"));
    }

    [Fact]
    public void RendersEmptyMessage_WhenNoMovementsAreProvided()
    {
        var cut = RenderComponent<MovementList>(parameters => parameters
            .Add(component => component.Items, Array.Empty<MovementListItem>())
            .Add(component => component.EmptyMessage, "No movements."));

        cut.Find("[data-testid='movement-list-empty']").TextContent.Should().Be("No movements.");
    }
}
