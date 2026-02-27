using FluentAssertions;
using ShiftMate.Application.Users.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class UpdateProfileHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        var context = TestDbContextFactory.Create();
        var handler = new UpdateProfileHandler(context);

        var command = new UpdateProfileCommand
        {
            UserId = Guid.NewGuid(),
            FirstName = "New",
            LastName = "Name",
            Email = "new@test.com"
        };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Användaren hittades inte.");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Update_Profile_Successfully()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var userId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Old", LastName = "Name",
            Email = "old@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProfileHandler(context);
        var command = new UpdateProfileCommand
        {
            UserId = userId,
            FirstName = "New",
            LastName = "Newsson",
            Email = "new@test.com"
        };

        await handler.Handle(command, CancellationToken.None);

        var user = context.Users.First(u => u.Id == userId);
        user.FirstName.Should().Be("New");
        user.LastName.Should().Be("Newsson");
        user.Email.Should().Be("new@test.com");

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
