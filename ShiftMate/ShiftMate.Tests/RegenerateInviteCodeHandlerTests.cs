using FluentAssertions;
using ShiftMate.Application.Organizations.Commands;
using ShiftMate.Domain.Entities;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class RegenerateInviteCodeHandlerTests
{
    [Fact]
    public async Task Handle_Should_Generate_New_Code()
    {
        var context = TestDbContextFactory.Create();
        var orgId = Guid.NewGuid();
        context.Organizations.Add(new Organization
        {
            Id = orgId,
            Name = "Test Org",
            InviteCode = "OLDCODE1",
            InviteCodeGeneratedAt = DateTime.UtcNow.AddDays(-10)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new RegenerateInviteCodeHandler(context);
        var result = await handler.Handle(new RegenerateInviteCodeCommand(orgId), CancellationToken.None);

        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(8);
        result.Should().NotBe("OLDCODE1");

        var org = context.Organizations.First();
        org.InviteCode.Should().Be(result);
        org.InviteCodeGeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Org_Not_Found()
    {
        var context = TestDbContextFactory.Create();
        var handler = new RegenerateInviteCodeHandler(context);

        await FluentActions.Invoking(() => handler.Handle(new RegenerateInviteCodeCommand(Guid.NewGuid()), CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*hittades inte*");

        TestDbContextFactory.Destroy(context);
    }
}
