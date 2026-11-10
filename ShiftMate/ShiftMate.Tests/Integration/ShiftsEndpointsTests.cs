using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Tests.Integration;

// Testar /api/shifts genom riktiga HTTP-anrop: autentisering, roller, felmappning,
// validering och att organisationer inte ser varandras data.
public class ShiftsEndpointsTests : IClassFixture<ShiftMateApiFactory>
{
    private readonly ShiftMateApiFactory _factory;

    public ShiftsEndpointsTests(ShiftMateApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetShifts_Should_Return_401_When_No_Token_Is_Sent()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/shifts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteShift_Should_Return_403_When_User_Is_Employee()
    {
        // Arrange
        var employee = await _factory.CreateUserAsync(Role.Employee);
        var shift = await _factory.CreateShiftAsync(employee.OrganizationId!.Value);
        var client = _factory.CreateAuthenticatedClient(employee);

        // Act
        var response = await client.DeleteAsync($"/api/shifts/{shift.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminCreateShift_Should_Return_400_With_Details_When_EndTime_Is_Before_StartTime()
    {
        // Arrange
        var manager = await _factory.CreateUserAsync(Role.Manager);
        var client = _factory.CreateAuthenticatedClient(manager);
        var start = DateTime.UtcNow.AddDays(2);
        var invalidShift = new { StartTime = start, EndTime = start.AddHours(-2) };

        // Act
        var response = await client.PostAsJsonAsync("/api/shifts/admin", invalidShift);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("error").GetBoolean().Should().BeTrue();
        body.GetProperty("details").EnumerateArray().Select(d => d.GetString())
            .Should().Contain("Passet kan inte sluta innan det har börjat.");
    }

    [Fact]
    public async Task GetShifts_Should_Not_Return_Shifts_From_Another_Organization()
    {
        // Arrange
        var userInOrgA = await _factory.CreateUserAsync(Role.Employee);
        var userInOrgB = await _factory.CreateUserAsync(Role.Employee);
        var shiftInOrgA = await _factory.CreateShiftAsync(userInOrgA.OrganizationId!.Value, userInOrgA.Id);
        var shiftInOrgB = await _factory.CreateShiftAsync(userInOrgB.OrganizationId!.Value, userInOrgB.Id);
        var client = _factory.CreateAuthenticatedClient(userInOrgB);

        // Act
        var response = await client.GetAsync("/api/shifts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var ids = body.GetProperty("items").EnumerateArray().Select(s => s.GetProperty("id").GetGuid()).ToList();
        ids.Should().Contain(shiftInOrgB.Id);
        ids.Should().NotContain(shiftInOrgA.Id);
    }
}
