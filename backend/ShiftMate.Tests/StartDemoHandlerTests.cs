using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShiftMate.Application.Common.Exceptions;
using ShiftMate.Application.Demo;
using ShiftMate.Application.Demo.Commands.StartDemo;
using ShiftMate.Domain.Entities;
using ShiftMate.Domain.Enums;
using ShiftMate.Infrastructure;
using ShiftMate.Infrastructure.Services;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class StartDemoHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Demo_Sandbox_When_Role_Is_Employee()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = CreateHandler(context);

        // Act
        var token = await handler.Handle(new StartDemoCommand("Employee"), CancellationToken.None);

        // Assert – en demo-organisation med fem användare (en chef) och pass 14 dagar framåt
        var organization = await context.Organizations.SingleAsync();
        organization.IsDemo.Should().BeTrue();
        organization.Name.Should().StartWith("Demo · ");

        var users = await context.Users.Where(u => u.OrganizationId == organization.Id).ToListAsync();
        users.Should().HaveCount(5);
        users.Should().ContainSingle(u => u.Role == Role.Manager);
        users.Should().OnlyContain(u => u.Email.EndsWith($"@{DemoConstants.EmailDomain}") && u.IsEmailVerified);

        var shifts = await context.Shifts.Where(s => s.OrganizationId == organization.Id).ToListAsync();
        shifts.Should().HaveCount(14 * 3);
        shifts.Should().Contain(s => s.UserId == null, "lediga pass ska synas under Lediga Pass");
        shifts.Should().Contain(s => s.IsUpForSwap, "pass utlagda för byte ska finnas");

        // Token tillhör en anställd i sandlådan och har en väntande inkommande förfrågan
        var claims = ReadClaims(token);
        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Employee");
        claims.Should().Contain(c => c.Type == "IsDemo" && c.Value == "true");
        claims.Should().Contain(c => c.Type == "OrganizationId" && c.Value == organization.Id.ToString());
        var visitorId = Guid.Parse(claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
        (await context.SwapRequests.CountAsync(sr => sr.TargetUserId == visitorId && sr.Status == SwapRequestStatus.Pending))
            .Should().Be(1);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Manager_Token_When_Role_Is_Manager()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = CreateHandler(context);

        // Act
        var token = await handler.Handle(new StartDemoCommand("Manager"), CancellationToken.None);

        // Assert
        var claims = ReadClaims(token);
        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Manager");
        var visitorId = Guid.Parse(claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
        (await context.SwapRequests.CountAsync(sr => sr.TargetUserId == visitorId && sr.Status == SwapRequestStatus.Pending))
            .Should().Be(1);

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_ValidationException_When_Role_Is_SuperAdmin()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = CreateHandler(context);

        // Act
        var act = () => handler.Handle(new StartDemoCommand("SuperAdmin"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        (await context.Organizations.AnyAsync()).Should().BeFalse();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Delete_Only_Expired_Demo_Organizations_When_Starting_New_Demo()
    {
        // Arrange – en utgången sandlåda, en aktiv sandlåda och en gammal riktig organisation
        var context = TestDbContextFactory.Create();
        var expired = AddOrganization(context, isDemo: true, createdAt: DateTime.UtcNow.AddHours(-3));
        var active = AddOrganization(context, isDemo: true, createdAt: DateTime.UtcNow.AddHours(-1));
        var real = AddOrganization(context, isDemo: false, createdAt: DateTime.UtcNow.AddDays(-30));
        await context.SaveChangesAsync();
        var handler = CreateHandler(context);

        // Act
        await handler.Handle(new StartDemoCommand("Employee"), CancellationToken.None);

        // Assert
        var remaining = await context.Organizations.Select(o => o.Id).ToListAsync();
        remaining.Should().NotContain(expired.Id);
        remaining.Should().Contain(active.Id);
        remaining.Should().Contain(real.Id, "städningen får aldrig röra riktiga organisationer");
        (await context.Users.AnyAsync(u => u.OrganizationId == expired.Id)).Should().BeFalse();
        (await context.Users.AnyAsync(u => u.OrganizationId == real.Id)).Should().BeTrue();

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Throw_TooManyRequests_When_Max_Active_Sandboxes_Is_Reached()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        for (var i = 0; i < DemoConstants.MaxActiveSandboxes; i++)
        {
            AddOrganization(context, isDemo: true, createdAt: DateTime.UtcNow);
        }
        await context.SaveChangesAsync();
        var handler = CreateHandler(context);

        // Act
        var act = () => handler.Handle(new StartDemoCommand("Employee"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TooManyRequestsException>().WithMessage("*full*");
        (await context.Organizations.CountAsync()).Should().Be(DemoConstants.MaxActiveSandboxes);

        TestDbContextFactory.Destroy(context);
    }

    private static Organization AddOrganization(AppDbContext context, bool isDemo, DateTime createdAt)
    {
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = $"Org-{Guid.NewGuid()}",
            InviteCode = Guid.NewGuid().ToString("N")[..8],
            CreatedAt = createdAt,
            IsDemo = isDemo
        };
        context.Organizations.Add(organization);
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Användare",
            Email = $"{Guid.NewGuid()}@test.se",
            PasswordHash = "hash",
            Role = Role.Employee,
            OrganizationId = organization.Id
        });
        return organization;
    }

    private static StartDemoCommandHandler CreateHandler(AppDbContext context)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256!!" },
                { "Jwt:Issuer", "ShiftMate.Tests" },
                { "Jwt:Audience", "ShiftMate.Tests" }
            })
            .Build();
        return new StartDemoCommandHandler(context, new JwtTokenGenerator(configuration), new StartDemoCommandValidator());
    }

    private static List<Claim> ReadClaims(string token) =>
        new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.ToList();
}
