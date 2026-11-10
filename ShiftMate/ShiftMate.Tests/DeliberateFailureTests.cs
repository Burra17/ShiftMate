using FluentAssertions;

namespace ShiftMate.Tests;

// Tillfälligt test för att se CI stoppa en PR – tas bort direkt efteråt
public class DeliberateFailureTests
{
	[Fact]
	public void Ci_Should_Fail_When_A_Test_Fails() => true.Should().BeFalse();
}