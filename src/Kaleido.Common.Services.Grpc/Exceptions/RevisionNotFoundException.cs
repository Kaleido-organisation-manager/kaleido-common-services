namespace Kaleido.Common.Services.Grpc.Exceptions;

public class RevisionNotFoundException : Exception
{
    public RevisionNotFoundException(string message) : base(message) { }
}