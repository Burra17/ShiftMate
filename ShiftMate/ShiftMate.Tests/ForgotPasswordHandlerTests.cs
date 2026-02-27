using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShiftMate.Application.Interfaces;
using ShiftMate.Application.Users.Commands;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class ForgotPasswordHandlerTests
{
    private static readonly Guid OrgId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Return_Silently_When_Email_Not_Found()
    {
        var context = TestDbContextFactory.Create();
        var handler = CreateHandler(context, out var emailMock);

        var command = new ForgotPasswordCommand { Email = "nonexistent@test.com" };

        await handler.Handle(command, CancellationToken.None);

        emailMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Set_Token_And_Send_Email_When_User_Exists()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var userId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = Role.Employee, OrganizationId = OrgId
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context, out var emailMock);
        var command = new ForgotPasswordCommand { Email = "test@test.com" };

        await handler.Handle(command, CancellationToken.None);

        var user = context.Users.First(u => u.Id == userId);
        user.ResetTokenHash.Should().NotBeNullOrEmpty();
        user.ResetTokenExpiresAt.Should().NotBeNull();
        user.ResetTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);

        emailMock.Verify(e => e.SendEmailAsync("test@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Find_User_Case_Insensitively()
    {
        var context = TestDbContextFactory.Create();
        SeedOrg(context);
        var userId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId, FirstName = "Test", LastName = "Testsson",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = Role.Employee, OrganizationId = OrgId
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context, out var emailMock);
        var command = new ForgotPasswordCommand { Email = "TEST@TEST.COM" };

        await handler.Handle(command, CancellationToken.None);

        emailMock.Verify(e => e.SendEmailAsync("test@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        TestDbContextFactory.Destroy(context);
    }

    private static ForgotPasswordHandler CreateHandler(
        Infrastructure.AppDbContext context,
        out Mock<IEmailService> emailMock)
    {
        emailMock = new Mock<IEmailService>();
        var loggerMock = new Mock<ILogger<ForgotPasswordHandler>>();
        return new ForgotPasswordHandler(context, emailMock.Object, loggerMock.Object);
    }

    private static void SeedOrg(Infrastructure.AppDbContext context)
    {
        context.Organizations.Add(new Organization { Id = OrgId, Name = "Test Org" });
        context.SaveChanges();
    }
}
