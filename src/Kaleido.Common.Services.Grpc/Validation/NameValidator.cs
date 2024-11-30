using FluentValidation;

namespace Kaleido.Common.Services.Grpc.Validation;

public class NameValidator : AbstractValidator<string>
{
    public NameValidator()
    {
        RuleFor(x => x).NotNull().NotEmpty().MaximumLength(100);
    }
}