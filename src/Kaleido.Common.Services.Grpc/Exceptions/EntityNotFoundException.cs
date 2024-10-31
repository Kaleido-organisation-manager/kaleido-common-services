namespace Kaleido.Common.Services.Grpc.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }
}