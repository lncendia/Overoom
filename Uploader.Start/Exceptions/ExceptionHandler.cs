using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Uploader.Start.Exceptions;

/// <summary>
/// Обработчик исключений, реализующий интерфейс IExceptionHandler.
/// </summary>
public class ExceptionHandler : IExceptionHandler
{
  /// <summary>
  /// Метод обработки исключения.
  /// </summary>
  /// <param name="context">Контекст HTTP-запроса.</param>
  /// <param name="exception">Исключение, которое необходимо обработать.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Асинхронная задача, возвращающая true, если исключение обработано.</returns>
  public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
    CancellationToken cancellationToken)
  {
    string? message;
    HttpStatusCode statusCode;

    var extensions = new Dictionary<string, object?>
    {
      ["traceId"] = context.TraceIdentifier
    };

    switch (exception)
    {
      default:
        statusCode = HttpStatusCode.InternalServerError;
        message = "Возникла ошибка при обработке запроса";
        break;
    }

    context.Response.StatusCode = (int)statusCode;

    var problemDetails = new ProblemDetails
    {
      Title = "Ошибка",
      Type = exception.GetType().Name.Replace("Exception", ""),
      Detail = message,
      Instance = context.Request.Path,
      Status = context.Response.StatusCode,
      Extensions = extensions
    };

    await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

    return true;
  }
}