using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ShiftMate.Application.Demo;
using ShiftMate.Application.Interfaces;
using ShiftMate.Infrastructure.Services;

namespace ShiftMate.Tests;

public class DemoSafeEmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_Should_Skip_When_Recipient_Is_Demo_Address()
    {
        // Arrange
        var inner = new Mock<IEmailService>();
        var service = new DemoSafeEmailService(inner.Object, NullLogger<DemoSafeEmailService>.Instance);

        // Act
        await service.SendEmailAsync($"anna@{DemoConstants.EmailDomain}", "Ämne", "Text");

        // Assert
        inner.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendEmailAsync_Should_Skip_When_Recipient_Has_Any_Invalid_Domain_Regardless_Of_Case()
    {
        // Arrange
        var inner = new Mock<IEmailService>();
        var service = new DemoSafeEmailService(inner.Object, NullLogger<DemoSafeEmailService>.Instance);

        // Act
        await service.SendEmailAsync("someone@example.INVALID", "Ämne", "Text");

        // Assert
        inner.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendEmailAsync_Should_Forward_When_Recipient_Is_Real_Address()
    {
        // Arrange
        var inner = new Mock<IEmailService>();
        var service = new DemoSafeEmailService(inner.Object, NullLogger<DemoSafeEmailService>.Instance);

        // Act
        await service.SendEmailAsync("anna@foretag.se", "Ämne", "Text");

        // Assert
        inner.Verify(e => e.SendEmailAsync("anna@foretag.se", "Ämne", "Text"), Times.Once);
    }
}
