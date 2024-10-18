using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models.Validations;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Models.Validations;

public class ValidationErrorTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var path = new[] { "property1", "property2" };
        var errorType = ValidationErrorType.NotFound;
        var errorMessage = "Entity not found";

        // Act
        var validationError = new ValidationError(path, errorType, errorMessage);

        // Assert
        Assert.Equal(path, validationError.Path);
        Assert.Equal(errorType, validationError.Type);
        Assert.Equal(errorMessage, validationError.Error);
    }

    [Fact]
    public void PrependPath_AddsPrefixToPath()
    {
        // Arrange
        var path = new[] { "property1", "property2" };
        var prefix = new[] { "root" };

        // Act
        var validationError = new ValidationError(path, ValidationErrorType.NotFound, "Entity not found");
        validationError.PrependPath(prefix);

        // Assert
        Assert.Equal(new[] { "root", "property1", "property2" }, validationError.Path);
    }

    [Fact]
    public void PrependPath_DoesNotChangePath_WhenPrefixIsEmpty()
    {
        // Arrange
        var path = new[] { "property1", "property2" };
        var validationError = new ValidationError(path, ValidationErrorType.NotFound, "Entity not found");

        // Act
        validationError.PrependPath([]);

        // Assert
        Assert.Equal(path, validationError.Path);
    }

    [Fact]
    public void PrependPath_AddsPrefixToEmptyPath()
    {
        // Arrange
        var prefix = new[] { "root" };
        var validationError = new ValidationError([], ValidationErrorType.NotFound, "Entity not found");

        // Act
        validationError.PrependPath(prefix);

        // Assert
        Assert.Equal(prefix, validationError.Path);
    }

    [Fact]
    public void Equals_ReturnsTrueForEqualErrors()
    {
        // Arrange
        var error1 = new ValidationError(new[] { "property" }, ValidationErrorType.NotFound, "Entity not found");
        var error2 = new ValidationError(new[] { "property" }, ValidationErrorType.NotFound, "Entity not found");

        // Act & Assert
        Assert.True(error1.Equals(error2));
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentErrors()
    {
        // Arrange
        var error1 = new ValidationError(new[] { "property1" }, ValidationErrorType.NotFound, "Entity not found");
        var error2 = new ValidationError(new[] { "property2" }, ValidationErrorType.AlreadyExists, "Entity already exists");

        // Act & Assert
        Assert.NotEqual(error1, error2);
        Assert.False(error1.Equals(error2));
    }

    [Fact]
    public void GetHashCode_ReturnsSameValueForEqualErrors()
    {
        // Arrange
        var error1 = new ValidationError(new[] { "property" }, ValidationErrorType.NotFound, "Entity not found");
        var error2 = new ValidationError(new[] { "property" }, ValidationErrorType.NotFound, "Entity not found");

        // Act & Assert
        Assert.Equal(error1.GetHashCode(), error2.GetHashCode());
    }
}
