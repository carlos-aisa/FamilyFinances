using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Payees.Abstractions;
using FamilyFinances.Application.Ledger.Payees.Handlers;
using FamilyFinances.Application.Ledger.Payees.Requests;
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

        repo.Setup(r => r.AddAsync(It.Is<Payee>(p =>
                p.Name == "Netflix" &&
                p.NormalizedName == "NETFLIX"), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreatePayeeHandler(repo.Object, uow.Object);

        var cmd = new CreatePayeeRequest(Name: "  Netflix  ");

        var id = await handler.HandleAsync(cmd, CancellationToken.None);

        id.Value.Should().NotBeEmpty();

        repo.Verify(r => r.GetByNormalizedNameAsync("NETFLIX", It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.AddAsync(It.IsAny<Payee>(), It.IsAny<CancellationToken>()), Times.Once);
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

        var cmd = new CreatePayeeRequest(Name: "Netflix");

        var act = async () => await handler.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();

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

        var cmd = new CreatePayeeRequest(Name: "   ");

        var act = async () => await handler.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();

        repo.Verify(r => r.GetByNormalizedNameAsync("", It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }
}
