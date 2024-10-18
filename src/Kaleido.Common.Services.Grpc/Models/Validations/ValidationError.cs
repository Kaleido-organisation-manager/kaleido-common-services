using Kaleido.Common.Services.Grpc.Constants;

namespace Kaleido.Common.Services.Grpc.Models.Validations;

public class ValidationError(IEnumerable<string> path, ValidationErrorType errorType, string errorMessage)
{
    public ValidationErrorType Type { get; } = errorType;
    public string Error { get; } = errorMessage;
    public IEnumerable<string> Path { get; private set; } = path;

    public ValidationError PrependPath(IEnumerable<string> prefix)
    {
        if (Path.Any())
        {
            Path = prefix.Concat(Path);
        }
        else
        {
            Path = prefix;
        }
        return this;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var other = (ValidationError)obj;
        return Type == other.Type && Error == other.Error && Path.SequenceEqual(other.Path);
    }

    public override int GetHashCode()
    {
        var hascodeStart = HashCode.Combine(Type, Error);
        foreach (var path in Path)
        {
            hascodeStart = HashCode.Combine(hascodeStart, path);
        }
        return hascodeStart;
    }
}
