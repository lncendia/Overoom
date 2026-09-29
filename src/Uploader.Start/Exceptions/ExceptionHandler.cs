using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Uploader.Application.Abstractions.Jobs;

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
      case JobNotFoundException ex:
        statusCode = HttpStatusCode.NotFound;
        message = "Задача не найдена";
        extensions["jobId"] = ex.JobId;
        break;

      case JobStateConflictException ex:
        statusCode = HttpStatusCode.Conflict;
        message = ex.Action switch
        {
          "cancel" => "Отменить можно только ожидающую или выполняющуюся задачу",
          "retry" => "Повторить можно только упавшую или отменённую задачу",
          "delete" => "Удалить можно только завершённую, упавшую или отменённую задачу",
          _ => "Действие недоступно в текущем состоянии задачи"
        };
        extensions["jobId"] = ex.JobId;
        extensions["status"] = ex.Status.ToString();
        break;

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