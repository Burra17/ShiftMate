using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShiftMate.Infrastructure;

namespace ShiftMate.Tests.Integration;

// Testar /health (liveness) och /health/db (readiness).
// /health får aldrig bero på databasen – Render startar om API:et om den inte svarar 200.
public class HealthEndpointsTests : IClassFixture<ShiftMateApiFactory>
{
    private readonly ShiftMateApiFactory _factory;

    public HealthEndpointsTests(ShiftMateApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_Should_Return_200_Without_Token()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthDb_Should_Return_200_Healthy_When_Database_Responds()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/db");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthDb_Should_Return_503_When_Database_Is_Unreachable()
    {
        // Arrange – byt InMemory mot en PostgreSQL-anslutning som inte går att nå (port 1)
        var client = _factory
            .WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<DbContextOptions<AppDbContext>>();
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(
                            "Host=127.0.0.1;Port=1;Database=unreachable;Username=test;Password=test;Timeout=1"
                        )
                    );
                })
            )
            .CreateClient();

        // Act
        var response = await client.GetAsync("/health/db");

        // Assert – och /health svarar fortfarande, eftersom den inte rör databasen
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await response.Content.ReadAsStringAsync()).Should().Be("Unhealthy");
        (await client.GetAsync("/health")).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
