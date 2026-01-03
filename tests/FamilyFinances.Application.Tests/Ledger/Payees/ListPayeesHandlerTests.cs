using FamilyFinances.Application.Ledger.Payees.Abstractions;
using FamilyFinances.Application.Ledger.Payees.Handlers;
using FamilyFinances.Application.Ledger.Payees.Requests;
using FamilyFinances.Domain.Ledger.Payees;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Payees;

public sealed class ListPayeesHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPayees()
    {
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);

        var p1 = Payee.Create("Netflix");
        var p2 = Payee.Create("Mercadona");

        repo.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payee> { p1, p2 });

        var handler = new ListPayeesHandler(repo.Object);

        var result = await handler.HandleAsync(new ListPayeesRequest(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(p1.Id);
        result[0].Name.Should().Be("Netflix");
        result[1].Id.Should().Be(p2.Id);
        result[1].Name.Should().Be("Mercadona");

        repo.Verify(r => r.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }
}
