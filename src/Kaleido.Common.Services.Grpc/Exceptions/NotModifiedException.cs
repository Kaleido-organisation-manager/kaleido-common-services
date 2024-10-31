namespace Kaleido.Common.Services.Grpc.Exceptions;

public class NotModifiedException : Exception
{
    public NotModifiedException(string message) : base(message) { }
}