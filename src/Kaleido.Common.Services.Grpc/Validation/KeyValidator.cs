using FluentValidation;

namespace Kaleido.Common.Services.Grpc.Validation;

public class KeyValidator : AbstractValidator<string>
{
    public KeyValidator()
    {
        RuleFor(x => x).NotNull().NotEmpty().Must(x => Guid.TryParse(x, out _));
    }
}