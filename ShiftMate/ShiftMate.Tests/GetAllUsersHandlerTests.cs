using FluentAssertions;
using ShiftMate.Application.Users.Queries;
using ShiftMate.Domain;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class GetAllUsersHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_All_Users_As_Dtos()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "secrethash", Role = Role.Employee
        });
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Eriksson",
            Email = "erik@test.com", PasswordHash = "secrethash", Role = Role.Admin
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllUsersHandler(context);

        // Act
        var result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Email == "anna@test.com");
        result.Should().Contain(u => u.Email == "erik@test.com");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Not_Expose_PasswordHash()
    {
        // Arrange — UserDto saknar PasswordHash-fält, så detta testar att mappningen
        // faktiskt använder DTO och inte returnerar hela User-entiteten
        var context = TestDbContextFactory.Create();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Svensson",
            Email = "anna@test.com", PasswordHash = "supersecret123", Role = Role.Employee
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllUsersHandler(context);

        // Act
        var result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        // Assert — resultatet ska vara UserDto som inte har PasswordHash-property
        result.Should().HaveCount(1);
        var userDto = result[0];
        userDto.FirstName.Should().Be("Anna");
        userDto.Email.Should().Be("anna@test.com");

        // Verifiera att typen är UserDto (som inte har PasswordHash)
        var properties = userDto.GetType().GetProperties();
        properties.Should().NotContain(p => p.Name == "PasswordHash");

        TestDbContextFactory.Destroy(context);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Users()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var handler = new GetAllUsersHandler(context);

        // Act
        var result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(context);
    }
}
