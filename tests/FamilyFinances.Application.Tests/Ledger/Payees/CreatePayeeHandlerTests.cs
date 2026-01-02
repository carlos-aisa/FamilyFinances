using FamilyFinances.Application.Abstractions;
using FamilyFinances.Application.Ledger.Payees.Create;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Payees;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Payees;

public sealed class CreatePayeeHandlerTests
{
    [Fact]
    public async Task Handle_CreatesPayee_AndPersistsIt()
    {
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByNormalizedNameAsync("NETFLIX", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payee?)null);

        repo.Setup(r => r.Add(It.Is<Payee>(p =>
                p.Name == "Netflix" &&
                p.NormalizedName == "NETFLIX")))
            .Verifiable();

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreatePayeeHandler(repo.Object, uow.Object);

        var cmd = new CreatePayeeCommand(Name: "  Netflix  ");

        var id = await handler.Handle(cmd, CancellationToken.None);

        id.Value.Should().NotBeEmpty();

        repo.Verify(r => r.GetByNormalizedNameAsync("NETFLIX", It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.Add(It.IsAny<Payee>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ThrowsDomainException_WhenPayeeAlreadyExists()
    {
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var existing = Payee.Create("Netflix");

        repo.Setup(r => r.GetByNormalizedNameAsync("NETFLIX", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var handler = new CreatePayeeHandler(repo.Object, uow.Object);

        var cmd = new CreatePayeeCommand(Name: "Netflix");

        var act = async () => await handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();

        repo.Verify(r => r.GetByNormalizedNameAsync("NETFLIX", It.IsAny<CancellationToken>()), Times.Once);

        // IMPORTANT: No persistence calls should happen
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_PropagatesDomainException_ForInvalidName()
    {
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByNormalizedNameAsync("", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payee?)null);

        var handler = new CreatePayeeHandler(repo.Object, uow.Object);

        var cmd = new CreatePayeeCommand(Name: "   ");

        var act = async () => await handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();

        repo.Verify(r => r.GetByNormalizedNameAsync("", It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }
}
