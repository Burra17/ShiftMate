using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ShiftMate.Domain.Enums;

namespace ShiftMate.Tests.Integration;

// Testar /api/swaprequests genom riktiga HTTP-anrop.
public class SwapRequestsEndpointsTests : IClassFixture<ShiftMateApiFactory>
{
    private readonly ShiftMateApiFactory _factory;

    public SwapRequestsEndpointsTests(ShiftMateApiFactory factory)
    {
        _factory = factory;
    }

    // Verifierar att ExceptionHandlingMiddleware mappar NotFoundException till 404 med felformatet
    [Fact]
    public async Task InitiateSwap_Should_Return_404_With_Error_Body_When_Shift_Does_Not_Exist()
    {
        // Arrange
        var employee = await _factory.CreateUserAsync(Role.Employee);
        var client = _factory.CreateAuthenticatedClient(employee);

        // Act
        var response = await client.PostAsJsonAsync("/api/swaprequests/initiate", new { ShiftId = Guid.NewGuid() });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("error").GetBoolean().Should().BeTrue();
        body.GetProperty("message").GetString().Should().Be("Passet hittades inte.");
    }
}
