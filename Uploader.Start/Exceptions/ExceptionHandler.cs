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
        // Переменная для хранения сообщения об ошибке, которое будет отправлено клиенту
        string? message;

        // Переменная для хранения статус кода
        HttpStatusCode statusCode;

        // Создаем словарь для хранения дополнительных данных, которые будут включены в ответ
        var extensions = new Dictionary<string, object?>
        {
            // Добавляем идентификатор запроса (traceId) для отслеживания ошибки
            ["traceId"] = context.TraceIdentifier
        };

        // Обработка исключения в зависимости от его типа
        switch (exception)
        {
            // Если исключение не относится к указанным выше типам
            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "Возникла ошибка при обработке запроса";
                break;
        }

        // Устанавливаем статус код ответа в HTTP-контексте
        context.Response.StatusCode = (int)statusCode;

        // Создаем объект ProblemDetails для формирования ответа клиенту
        var problemDetails = new ProblemDetails
        {
            Title = "Ошибка",
            Type = exception.GetType().Name.Replace("Exception", ""),
            Detail = message,
            Instance = context.Request.Path,
            Status = context.Response.StatusCode,
            Extensions = extensions
        };

        // Отправляем ответ в формате JSON клиенту
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Возвращаем true, чтобы указать, что исключение успешно обработано
        return true;
    }
}