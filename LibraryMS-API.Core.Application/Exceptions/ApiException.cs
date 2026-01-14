using System.Globalization;
using System.Net;

namespace LibraryMS_API.Core.Application.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }
        public ApiException() : base() { }
        public ApiException(string message) : base(message) { }
        public ApiException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public ApiException(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args)) { }

        // Helper methods for common scenarios
        public static ApiException NotFound(string message)
            => new(message, (int)HttpStatusCode.NotFound);

        public static ApiException Conflict(string message)
            => new(message, (int)HttpStatusCode.Conflict);

        public static ApiException BadRequest(string message)
            => new(message, (int)HttpStatusCode.BadRequest);

        public static ApiException Unauthorized(string message = "Unauthorized access")
            => new(message, (int)HttpStatusCode.Unauthorized);

        public static ApiException Forbidden(string message = "Access forbidden")
            => new(message, (int)HttpStatusCode.Forbidden);

        public static ApiException InternalServerError(string message = "Access forbidden")
          => new(message, (int)HttpStatusCode.InternalServerError);


    }
}
