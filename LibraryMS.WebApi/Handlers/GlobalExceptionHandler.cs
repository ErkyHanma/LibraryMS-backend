using LibraryMS.Core.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace LibraryMS.WebApi.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            string exceptionTitle = "An unexpected error occurred";
            string details = exception.Message;

            switch (exception)
            {
                case ApiException apiException:
                    switch (apiException.StatusCode)
                    {
                        case (int)HttpStatusCode.BadRequest:
                            exceptionTitle = "Bad Request";
                            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            break;
                        case (int)HttpStatusCode.InternalServerError:
                            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            break;
                        case (int)HttpStatusCode.NotFound:
                            exceptionTitle = "Not found";
                            httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            break;
                        case (int)HttpStatusCode.Unauthorized:
                            exceptionTitle = "Unauthorized";
                            httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            break;
                        case (int)HttpStatusCode.Forbidden:
                            exceptionTitle = "Forbidden";
                            httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            break;
                        default:
                            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            break;
                    }
                    break;
                case KeyNotFoundException:
                    exceptionTitle = "Not found";
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case ArgumentException:
                    exceptionTitle = "Bad Request";
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case ValidationException:
                    exceptionTitle = "Bad Request";
                    details = ((ValidationException)exception).Errors.Aggregate((a, b) => a + ", " + b);
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                default:
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    details = "Please try again later or contact support if the problem persists";
                    break;
            }

            var problemDetails = new
            {
                Title = exceptionTitle,
                Status = httpContext.Response.StatusCode,
                Detail = details,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);

            return true;
        }
    }
}
