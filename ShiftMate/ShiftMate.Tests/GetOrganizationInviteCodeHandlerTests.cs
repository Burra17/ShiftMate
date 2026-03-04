using FluentAssertions;
using ShiftMate.Application.Organizations.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetOrganizationInviteCodeHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_InviteCode()
    {
        var context = TestDbContextFactory.Create();
        var orgId = Guid.NewGuid();
        var generatedAt = DateTime.UtcNow.AddDays(-3);
        context.Organizations.Add(new Organization
        {
            Id = orgId,
            Name = "Test Org",
            InviteCode = "ABC12345",
            InviteCodeGeneratedAt = generatedAt
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetOrganizationInviteCodeHandler(context);
        var result = await handler.Handle(new GetOrganizationInviteCodeQuery(orgId), CancellationToken.None);

        result.InviteCode.Should().Be("ABC12345");
        result.OrganizationName.Should().Be("Test Org");
        result.GeneratedAt.Should().BeCloseTo(generatedAt, TimeSpan.FromSeconds(1));

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Org_Not_Found()
    {
        var context = TestDbContextFactory.Create();
        var handler = new GetOrganizationInviteCodeHandler(context);

        await FluentActions.Invoking(() => handler.Handle(new GetOrganizationInviteCodeQuery(Guid.NewGuid()), CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*hittades inte*");

        TestDbContextFactory.Destroy(context);
    }
}
