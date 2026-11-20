using System.Globalization;

namespace Qeq.Application.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; set; }

    public ApiException() : base()
    {
        StatusCode = 400;
    }

    public ApiException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public ApiException(string message, params object[] args)
        : base(string.Format(CultureInfo.CurrentCulture, message, args))
    {
        StatusCode = 400;
    }
}