namespace Qeq.Application.Exceptions;

public class LlmProviderException : Exception
{
    public int StatusCode { get; }

    public LlmProviderException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}