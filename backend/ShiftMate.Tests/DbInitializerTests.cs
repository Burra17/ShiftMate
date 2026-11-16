using FluentAssertions;
using ShiftMate.Domain.Entities;
using ShiftMate.Infrastructure;
using ShiftMate.Tests.Support;

namespace ShiftMate.Tests;

public class DbInitializerTests
{
    [Fact]
    public void Initialize_Should_Not_Create_Duplicates_When_Run_Twice()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        DbInitializer.Initialize(context);
        var usersAfterFirstRun = context.Users.Count();
        var shiftsAfterFirstRun = context.Shifts.Count();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        context.Users.Count().Should().Be(usersAfterFirstRun);
        context.Shifts.Count().Should().Be(shiftsAfterFirstRun);
    }

    [Fact]
    public void Initialize_Should_Not_Delete_Existing_Shifts_When_Run_Again()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        DbInitializer.Initialize(context);
        var org = context.Organizations.First(o => o.Name == "ShiftMate Demo");
        var ownShift = new Shift
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            StartTime = DateTime.UtcNow.Date.AddDays(10).AddHours(8),
            EndTime = DateTime.UtcNow.Date.AddDays(10).AddHours(16)
        };
        context.Shifts.Add(ownShift);
        context.SaveChanges();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        context.Shifts.Any(s => s.Id == ownShift.Id).Should().BeTrue();
    }

    [Fact]
    public void Initialize_Should_Seed_New_Shifts_When_No_Upcoming_Shifts_Exist()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        DbInitializer.Initialize(context);
        var shiftsAfterFirstRun = context.Shifts.Count();

        // Flytta alla pass bakåt i tiden så att inga kommande pass finns kvar
        foreach (var shift in context.Shifts)
        {
            shift.StartTime = shift.StartTime.AddDays(-30);
            shift.EndTime = shift.EndTime.AddDays(-30);
        }
        context.SaveChanges();

        // Act
        DbInitializer.Initialize(context);

        // Assert
        context.Shifts.Count().Should().Be(shiftsAfterFirstRun * 2);
    }
}
