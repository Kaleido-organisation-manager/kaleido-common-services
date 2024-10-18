# Kaleido Common Services gRPC

This project contains common services and utilities for gRPC-based microservices in the Kaleido ecosystem.

## Project Structure

The project is structured as follows:

- `src/Kaleido.Common.Services.Grpc`: Main library project
- `src/Kaleido.Common.Services.Grpc.Tests.Unit`: Unit tests for the library

## Features

### Base Repository

The project includes a generic base repository implementation that provides common CRUD operations for entities. It supports:

- Creating entities
- Retrieving active entities
- Updating entities
- Deleting entities (soft delete)
- Managing entity revisions
- Querying entities by status

### Validation

The project includes a robust validation system:

- `ValidationResult` class for aggregating validation errors
- `ValidationError` class for representing individual validation errors
- Support for different validation error types (e.g., NotFound, AlreadyExists, InvalidFormat)
- `IRequestValidator<T>` interface for implementing request validators

### Exception Handling

Custom `ValidationException` for handling validation errors in gRPC context.

### Entity Model

`BaseEntity` class providing common properties for all entities:

- Id
- Key
- CreatedAt
- Revision
- Status

### Constants

- `EntityStatus` enum for representing entity states (Active, Deleted, Archived)
- `ValidationErrorType` enum for categorizing validation errors

### Handlers

`IBaseHandler<TRequest, TResponse>` interface for implementing request handlers with built-in validation support.

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio or your preferred IDE
3. Restore NuGet packages
4. Build the solution

## Running Tests

The project uses xUnit for unit testing. To run the tests:

1. Open Test Explorer in Visual Studio
2. Click "Run All" or run individual test classes

Alternatively, you can run tests from the command line:

```bash
dotnet test src/Kaleido.Common.Services.Grpc.Tests.Unit/Kaleido.Common.Services.Grpc.Tests.Unit.csproj
```

## CI/CD

The project includes GitHub Actions workflows for continuous integration:

- PR workflow: Builds the project and runs unit tests for pull requests
- Merge workflow: Runs on merges to `develop` and `main` branches, includes versioning using GitVersion

## Dependencies

- .NET 8.0
- Entity Framework Core
- gRPC for ASP.NET Core
- xUnit for testing

For a complete list of dependencies, refer to the `.csproj` files in the `src` directory.

## Contributing

Please refer to the repository's contribution guidelines for information on how to contribute to this project.