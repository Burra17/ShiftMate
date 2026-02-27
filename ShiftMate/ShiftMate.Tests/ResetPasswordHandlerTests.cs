using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ShiftMate.Application.Users.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class ResetPasswordHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();
    private const string ErrorMessage = "Ogiltig eller utgången återställningslänk.";

    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        var context = TestDbContextFactory.Create();
        var handler = CreateHandler(context);

        var command = new ResetPasswordCommand
        {
            Token = "sometoken",
            Email = "nonexistent@test.com",
            NewPassword = "newpassword123"
        };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage(ErrorMessage);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_No_Token_Stored()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = Role.Employee, OrganizationId = OrgId,
            ResetTokenHash = null,
            ResetTokenExpiresAt = null
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new ResetPasswordCommand
        {
            Token = "sometoken",
            Email = "test@test.com",
            NewPassword = "newpassword123"
        };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage(ErrorMessage);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Token_Is_Expired()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var token = "testtoken123";

        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = Role.Employee, OrganizationId = OrgId,
            ResetTokenHash = BCrypt.Net.BCrypt.HashPassword(token),
            ResetTokenExpiresAt = DateTime.UtcNow.AddHours(-1)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new ResetPasswordCommand
        {
            Token = token,
            Email = "test@test.com",
            NewPassword = "newpassword123"
        };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage(ErrorMessage);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Token_Is_Wrong()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);

        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = Role.Employee, OrganizationId = OrgId,
            ResetTokenHash = BCrypt.Net.BCrypt.HashPassword("correcttoken"),
            ResetTokenExpiresAt = DateTime.UtcNow.AddHours(1)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new ResetPasswordCommand
        {
            Token = "wrongtoken",
            Email = "test@test.com",
            NewPassword = "newpassword123"
        };

        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage(ErrorMessage);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Reset_Password_And_Clear_Token_On_Success()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var userId = Guid.NewGuid();
        var token = "validtoken123";

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword"),
            Role = Role.Employee, OrganizationId = OrgId,
            ResetTokenHash = BCrypt.Net.BCrypt.HashPassword(token),
            ResetTokenExpiresAt = DateTime.UtcNow.AddHours(1)
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new ResetPasswordCommand
        {
            Token = token,
            Email = "test@test.com",
            NewPassword = "newpassword123"
        };

        await handler.Handle(command, CancellationToken.None);

        var user = context.Users.First(u => u.Id == userId);
        BCrypt.Net.BCrypt.Verify("newpassword123", user.PasswordHash).Should().BeTrue();
        BCrypt.Net.BCrypt.Verify("oldpassword", user.PasswordHash).Should().BeFalse();
        user.ResetTokenHash.Should().BeNull();
        user.ResetTokenExpiresAt.Should().BeNull();

        TestDbContextFactory.Destroy(context);
    }

    private static ResetPasswordHandler CreateHandler(Infrastructure.AppDbContext context)
    {
        var validatorMock = new Mock<IValidator<ResetPasswordCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return new ResetPasswordHandler(context, validatorMock.Object);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
