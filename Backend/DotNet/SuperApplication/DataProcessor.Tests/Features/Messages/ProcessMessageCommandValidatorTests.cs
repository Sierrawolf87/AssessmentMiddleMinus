using DataProcessor.Features.Messages;
using FluentAssertions;

namespace DataProcessor.Tests.Features.Messages;

public class ProcessMessageCommandValidatorTests
{
    private readonly ProcessMessageCommandValidator _validator;

    public ProcessMessageCommandValidatorTests()
    {
        _validator = new ProcessMessageCommandValidator();
    }

    [Fact]
    public void Validate_WithValidContent_ShouldPass()
    {
        // Arrange
        var command = new ProcessMessageCommand("valid content", "routing.key");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyContent_ShouldFail()
    {
        // Arrange
        var command = new ProcessMessageCommand(string.Empty, "routing.key");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Content is required.");
    }

    [Fact]
    public void Validate_WithNullContent_ShouldFail()
    {
        // Arrange
        var command = new ProcessMessageCommand(null!, "routing.key");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Content is required.");
    }

    [Fact]
    public void Validate_WithWhitespaceContent_ShouldFail()
    {
        // Arrange
        var command = new ProcessMessageCommand("   ", "routing.key");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }
}
