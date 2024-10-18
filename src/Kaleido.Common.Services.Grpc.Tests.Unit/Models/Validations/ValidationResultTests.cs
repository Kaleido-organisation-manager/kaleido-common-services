using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Models.Validations;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Models.Validations;

public class ValidationResultTests
{
    [Fact]
    public void IsValid_ReturnsFalse_WhenErrors()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddError(new[] { "property1", "property2" }, ValidationErrorType.NotFound, "Entity not found");

        // Assert
        Assert.False(validationResult.IsValid);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenNoErrors()
    {
        // Act
        var validationResult = new ValidationResult();

        // Assert
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public void AddError_AddsErrorToResult()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddError(new[] { "property1", "property2" }, ValidationErrorType.NotFound, "Entity not found");

        // Assert
        Assert.Single(validationResult.Errors);
        Assert.Equal(new[] { "property1", "property2" }, validationResult.Errors.First().Path);
        Assert.Equal(ValidationErrorType.NotFound, validationResult.Errors.First().Type);
        Assert.Equal("Entity not found", validationResult.Errors.First().Error);
    }

    [Fact]
    public void AddNotFoundError_AddsNotFoundErrorToResult()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddNotFoundError(new[] { "property1", "property2" }, "Entity not found");

        // Assert
        Assert.Single(validationResult.Errors);
        Assert.Equal(new[] { "property1", "property2" }, validationResult.Errors.First().Path);
        Assert.Equal(ValidationErrorType.NotFound, validationResult.Errors.First().Type);
        Assert.Equal("Entity not found", validationResult.Errors.First().Error);
    }

    [Fact]
    public void AddAlreadyExistsError_AddsAlreadyExistsErrorToResult()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddAlreadyExistsError(new[] { "property1", "property2" }, "Entity already exists");

        // Assert
        Assert.Single(validationResult.Errors);
        Assert.Equal(new[] { "property1", "property2" }, validationResult.Errors.First().Path);
        Assert.Equal(ValidationErrorType.AlreadyExists, validationResult.Errors.First().Type);
        Assert.Equal("Entity already exists", validationResult.Errors.First().Error);
    }

    [Fact]
    public void AddInvalidFormatError_AddsInvalidFormatErrorToResult()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddInvalidFormatError(new[] { "property1", "property2" }, "Invalid format");

        // Assert
        Assert.Single(validationResult.Errors);
        Assert.Equal(new[] { "property1", "property2" }, validationResult.Errors.First().Path);
        Assert.Equal(ValidationErrorType.InvalidFormat, validationResult.Errors.First().Type);
        Assert.Equal("Invalid format", validationResult.Errors.First().Error);
    }

    [Fact]
    public void AddDataConflictError_AddsDataConflictErrorToResult()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddDataConflictError(new[] { "property1", "property2" }, "Data conflict");

        // Assert
        Assert.Single(validationResult.Errors);
        Assert.Equal(new[] { "property1", "property2" }, validationResult.Errors.First().Path);
        Assert.Equal(ValidationErrorType.DataConflict, validationResult.Errors.First().Type);
        Assert.Equal("Data conflict", validationResult.Errors.First().Error);
    }

    [Fact]
    public void AddRequiredError_AddsRequiredErrorToResult()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddRequiredError(new[] { "property1", "property2" }, "Required");

        // Assert
        Assert.Single(validationResult.Errors);
        Assert.Equal(new[] { "property1", "property2" }, validationResult.Errors.First().Path);
        Assert.Equal(ValidationErrorType.Required, validationResult.Errors.First().Type);
        Assert.Equal("Required", validationResult.Errors.First().Error);
    }

    [Fact]
    public void PrependPath_PrependsPathToErrors()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddError(new[] { "property1", "property2" }, ValidationErrorType.NotFound, "Entity not found");
        validationResult.PrependPath(new[] { "root" });

        // Assert
        Assert.Single(validationResult.Errors);
        Assert.Equal(new[] { "root", "property1", "property2" }, validationResult.Errors.First().Path);
    }

    [Fact]
    public void PrependPath_PrependsPathToErrors_WhenMultipleErrors()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddError(new[] { "property1", "property2" }, ValidationErrorType.NotFound, "Entity not found");
        validationResult.AddError(new[] { "property3", "property4" }, ValidationErrorType.NotFound, "Entity not found");
        validationResult.PrependPath(new[] { "root" });

        // Assert   
        Assert.Equal(2, validationResult.Errors.Count());
        Assert.Equal(new[] { "root", "property1", "property2" }, validationResult.Errors.First().Path);
        Assert.Equal(new[] { "root", "property3", "property4" }, validationResult.Errors.Last().Path);
    }

    [Fact]
    public void PrependPath_PrependsPathToErrors_WhenPathIsEmpty()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddError(new[] { "property1", "property2" }, ValidationErrorType.NotFound, "Entity not found");
        validationResult.PrependPath([]);

        // Assert
        Assert.Single(validationResult.Errors);
        Assert.Equal(new[] { "property1", "property2" }, validationResult.Errors.First().Path);
    }

    [Fact]
    public void PrependPath_PrependsPathToErrors_WhenErrorPathIsEmpty()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddError([], ValidationErrorType.NotFound, "Entity not found");
        validationResult.PrependPath(new[] { "root" });

        // Assert
        Assert.Single(validationResult.Errors);
        Assert.Equal(new[] { "root" }, validationResult.Errors.First().Path);
    }

    [Fact]
    public void Merge_MergesTwoValidationResults()
    {
        // Arrange
        var validationResult = new ValidationResult();

        // Act
        validationResult.Merge(new ValidationResult());

        // Assert
        Assert.Empty(validationResult.Errors);
    }

    [Fact]
    public void Merge_MergesTwoValidationResults_WhenOneHasNoErrors()
    {
        // Act
        var validationResult1 = new ValidationResult();
        validationResult1.AddError(new[] { "property1", "property2" }, ValidationErrorType.NotFound, "Entity not found");

        var validationResult2 = new ValidationResult();

        validationResult1.Merge(validationResult2);

        // Assert
        Assert.Single(validationResult1.Errors);
        Assert.Equal(new[] { "property1", "property2" }, validationResult1.Errors.First().Path);
    }

    [Fact]
    public void Merge_MergesTwoValidationResults_WhenBothHaveErrors()
    {
        // Act
        var validationResult1 = new ValidationResult();
        validationResult1.AddError(new[] { "property1", "property2" }, ValidationErrorType.NotFound, "Entity not found");

        var validationResult2 = new ValidationResult();
        validationResult2.AddError(new[] { "property3", "property4" }, ValidationErrorType.NotFound, "Entity not found");

        validationResult1.Merge(validationResult2);

        // Assert
        Assert.Equal(2, validationResult1.Errors.Count());
        Assert.Equal(new[] { "property1", "property2" }, validationResult1.Errors.First().Path);
        Assert.Equal(new[] { "property3", "property4" }, validationResult1.Errors.Last().Path);
    }

    [Fact]
    public void ThrowIfInvalid_ThrowsValidationException_WhenErrors()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddError(new[] { "property1", "property2" }, ValidationErrorType.NotFound, "Entity not found");

        // Assert
        Assert.Throws<ValidationException>(() => validationResult.ThrowIfInvalid());
    }

    [Fact]
    public void ThrowIfInvalid_DoesNotThrowValidationException_WhenNoErrors()
    {
        // Act
        var validationResult = new ValidationResult();

        // Assert
        validationResult.ThrowIfInvalid();
    }

    [Fact]
    public void AddMultipleErrors_AddsAllErrorsToResult()
    {
        // Act
        var validationResult = new ValidationResult();
        validationResult.AddNotFoundError(new[] { "property1" }, "Entity not found");
        validationResult.AddAlreadyExistsError(new[] { "property2" }, "Entity already exists");
        validationResult.AddInvalidFormatError(new[] { "property3" }, "Invalid format");

        // Assert
        Assert.Equal(3, validationResult.Errors.Count());
        Assert.Contains(validationResult.Errors, e => e.Type == ValidationErrorType.NotFound);
        Assert.Contains(validationResult.Errors, e => e.Type == ValidationErrorType.AlreadyExists);
        Assert.Contains(validationResult.Errors, e => e.Type == ValidationErrorType.InvalidFormat);
    }

    [Fact]
    public void Merge_MergesTwoValidationResults_WithMultipleErrors()
    {
        // Arrange
        var validationResult1 = new ValidationResult();
        validationResult1.AddNotFoundError(new[] { "property1" }, "Entity not found");
        validationResult1.AddAlreadyExistsError(new[] { "property2" }, "Entity already exists");

        var validationResult2 = new ValidationResult();
        validationResult2.AddInvalidFormatError(new[] { "property3" }, "Invalid format");
        validationResult2.AddRequiredError(new[] { "property4" }, "Required");

        // Act
        validationResult1.Merge(validationResult2);

        // Assert
        Assert.Equal(4, validationResult1.Errors.Count());
        Assert.Contains(validationResult1.Errors, e => e.Type == ValidationErrorType.NotFound);
        Assert.Contains(validationResult1.Errors, e => e.Type == ValidationErrorType.AlreadyExists);
        Assert.Contains(validationResult1.Errors, e => e.Type == ValidationErrorType.InvalidFormat);
        Assert.Contains(validationResult1.Errors, e => e.Type == ValidationErrorType.Required);
    }

    [Fact]
    public void PrependPath_PrependsEmptyPathToErrors_WhenMultipleErrors()
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.AddError(new[] { "property1" }, ValidationErrorType.NotFound, "Entity not found");
        validationResult.AddError(new[] { "property2" }, ValidationErrorType.AlreadyExists, "Entity already exists");

        // Act
        validationResult.PrependPath([]);

        // Assert
        Assert.Equal(2, validationResult.Errors.Count());
        Assert.Equal(new[] { "property1" }, validationResult.Errors.First().Path);
        Assert.Equal(new[] { "property2" }, validationResult.Errors.Last().Path);
    }

    [Fact]
    public void ThrowIfInvalid_ThrowsValidationException_WithMultipleErrors()
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.AddNotFoundError(new[] { "property1" }, "Entity not found");
        validationResult.AddAlreadyExistsError(new[] { "property2" }, "Entity already exists");

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => validationResult.ThrowIfInvalid());
        Assert.Equal(2, exception.Errors.Count());
        Assert.Contains(exception.Errors, e => e.Type == ValidationErrorType.NotFound);
        Assert.Contains(exception.Errors, e => e.Type == ValidationErrorType.AlreadyExists);
    }
}
