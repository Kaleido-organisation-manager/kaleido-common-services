namespace Kaleido.Common.Services.Grpc.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    { }
}