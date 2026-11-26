using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;

namespace ShiftMate.Tests.Integration;

// Testar /api/demo/start: fungerar utan inloggning, ger en token som fungerar mot
// skyddade endpoints, och stoppar missbruk med rate limit.
public class DemoEndpointsTests : IClassFixture<ShiftMateApiFactory>
{
    private readonly ShiftMateApiFactory _factory;

    public DemoEndpointsTests(ShiftMateApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Start_Should_Return_Token_That_Works_Against_Protected_Endpoints_Without_Login()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/demo/start", new { Role = "Employee" });

        // Assert – token fungerar mot en skyddad endpoint och användaren har pass i sin sandlåda
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = body.GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var shiftsResponse = await client.GetAsync("/api/shifts/mine");
        shiftsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var shifts = await shiftsResponse.Content.ReadFromJsonAsync<JsonElement>();
        shifts.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Start_Should_Return_400_When_Role_Is_SuperAdmin()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/demo/start", new { Role = "SuperAdmin" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Start_Should_Return_429_When_Rate_Limit_Is_Reached()
    {
        // Arrange – egen host med gränsen 1 start per minut
        var client = _factory
            .WithWebHostBuilder(builder => builder.UseSetting("Demo:StartsPerMinute", "1"))
            .CreateClient();

        // Act
        var first = await client.PostAsJsonAsync("/api/demo/start", new { Role = "Manager" });
        var second = await client.PostAsJsonAsync("/api/demo/start", new { Role = "Manager" });

        // Assert
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        var body = await second.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("error").GetBoolean().Should().BeTrue();
        body.GetProperty("message").GetString().Should().Contain("försök igen");
    }
}
