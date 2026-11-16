using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Tests.Integration;

// Testar /api/users genom riktiga HTTP-anrop.
public class UsersEndpointsTests : IClassFixture<ShiftMateApiFactory>
{
    private readonly ShiftMateApiFactory _factory;

    public UsersEndpointsTests(ShiftMateApiFactory factory)
    {
        _factory = factory;
    }

    // Hela vägen: en token som API:et självt utfärdar vid login ska accepteras av skyddade endpoints.
    // Fångar fel i Issuer/Audience/nyckel/claims som tester med egengjorda tokens skulle missa.
    [Fact]
    public async Task Login_Should_Return_Token_That_Is_Accepted_By_Protected_Endpoints()
    {
        // Arrange
        var user = await _factory.CreateUserAsync(Role.Employee, password: "Secret123");
        var shift = await _factory.CreateShiftAsync(user.OrganizationId!.Value, user.Id);
        var client = _factory.CreateClient();

        // Act
        var loginResponse = await client.PostAsJsonAsync("/api/users/login",
            new { Email = user.Email, Password = "Secret123" });
        var token = (await loginResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var myShiftsResponse = await client.GetAsync("/api/shifts/mine");

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        myShiftsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var myShifts = await myShiftsResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
        myShifts!.Select(s => s.GetProperty("id").GetGuid()).Should().Contain(shift.Id);
    }
}
