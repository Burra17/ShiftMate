using FluentAssertions;
using ShiftMate.Application.Users.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class RegisterUserCommandHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Register_User_Successfully()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var handler = new RegisterUserCommandHandler(context);

        var command = new RegisterUserCommand("Test", "Testsson", "test@test.com", "password123", OrgId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.FirstName.Should().Be("Test");
        result.LastName.Should().Be("Testsson");
        result.Email.Should().Be("test@test.com");
        result.OrganizationId.Should().Be(OrgId);

        context.Users.Should().HaveCount(1);
        var user = context.Users.First();
        user.Role.Should().Be(Role.Employee);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Email_Already_Exists()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Existing", LastName = "User",
            Email = "test@test.com", PasswordHash = "hash", Role = Role.Employee, OrganizationId = OrgId
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new RegisterUserCommandHandler(context);
        var command = new RegisterUserCommand("Test", "Testsson", "Test@Test.com", "password123", OrgId);

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("*already exists*");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Store_Email_As_Lowercase()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var handler = new RegisterUserCommandHandler(context);

        var command = new RegisterUserCommand("Test", "Testsson", "Test@TEST.com", "password123", OrgId);

        await handler.Handle(command, CancellationToken.None);

        var user = context.Users.First();
        user.Email.Should().Be("test@test.com");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Hash_Password()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var handler = new RegisterUserCommandHandler(context);

        var command = new RegisterUserCommand("Test", "Testsson", "test@test.com", "password123", OrgId);

        await handler.Handle(command, CancellationToken.None);

        var user = context.Users.First();
        user.PasswordHash.Should().NotBe("password123");
        user.PasswordHash.Should().StartWith("$2");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Organization_Not_Found()
    {
        var context = TestDbContextFactory.Create();
        var handler = new RegisterUserCommandHandler(context);

        var command = new RegisterUserCommand("Test", "Testsson", "test@test.com", "password123", Guid.NewGuid());

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("*hittades inte*");

        TestDbContextFactory.Destroy(context);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
