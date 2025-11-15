using System.Net;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Films.Exceptions;
using Films.Domain.Rooms.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Films.Start.Exceptions;

/// <summary>
/// Обработчик исключений, реализующий интерфейс IExceptionHandler.
/// Обрабатывает все бизнес-исключения приложения и возвращает соответствующие HTTP-ответы.
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
            // NotFound исключения - 404 статус код
            case CommentNotFoundException ex:
                statusCode = HttpStatusCode.NotFound;
                message = "Комментарий не найден";
                extensions["commentId"] = ex.CommentId;
                break;

            case FilmNotFoundException ex:
                statusCode = HttpStatusCode.NotFound;
                message = "Фильм не найден";
                extensions["filmId"] = ex.FilmId;
                break;

            case PlaylistNotFoundException ex:
                statusCode = HttpStatusCode.NotFound;
                message = "Подборка не найдена";
                extensions["playlistId"] = ex.PlaylistId;
                break;

            case RoomNotFoundException ex:
                statusCode = HttpStatusCode.NotFound;
                message = "Комната не найдена";
                extensions["roomId"] = ex.RoomId;
                break;

            case UserNotFoundException ex:
                statusCode = HttpStatusCode.NotFound;
                message = "Пользователь не найден";
                extensions["userId"] = ex.UserId;
                break;

            // Conflict исключения - 409 статус код
            case FilmAlreadyExistsException ex:
                statusCode = HttpStatusCode.Conflict;
                message = "Фильм с таким названием и датой уже существует";
                extensions["name"] = ex.Name;
                extensions["date"] = ex.Date;
                break;

            case RatingAlreadyExistsException ex:
                statusCode = HttpStatusCode.Conflict;
                message = "Вы уже оценили этот фильм";
                extensions["filmId"] = ex.FilmId;
                extensions["userId"] = ex.UserId;
                break;
            
            case PlaylistAlreadyExistsException ex:
                statusCode = HttpStatusCode.Conflict;
                message = "Подборка с таким именем уже существует";
                extensions["name"] = ex.Name;
                break;

            case UserAlreadyExistsException ex:
                statusCode = HttpStatusCode.Conflict;
                message = "Пользователь уже существует";
                extensions["userId"] = ex.UserId;
                break;
            
            case UserAlreadyInRoomException ex:
                statusCode = HttpStatusCode.Forbidden;
                message = "Пользователь уже состоит в комнате";
                extensions["userId"] = ex.UserId;
                extensions["roomId"] = ex.RoomId;
                break;

            // Forbidden исключения - 403 статус код
            case ActionNotAllowedException ex:
                statusCode = HttpStatusCode.Forbidden;
                message = "Действие запрещено";
                extensions["roomId"] = ex.RoomId;
                extensions["action"] = ex.Action;
                break;

            case UserBannedInRoomException ex:
                statusCode = HttpStatusCode.Forbidden;
                message = "Пользователь заблокирован в комнате";
                extensions["userId"] = ex.UserId;
                extensions["roomId"] = ex.RoomId;
                break;

            // Bad Request исключения - 400 статус код
            case CommentNotBelongToUserException ex:
                statusCode = HttpStatusCode.BadRequest;
                message = "Комментарий не принадлежит пользователю";
                extensions["userId"] = ex.UserId;
                extensions["commentId"] = ex.CommentId;
                break;

            case EmptyTagsCollectionException ex:
                statusCode = HttpStatusCode.BadRequest;
                message = "Коллекция тегов не может быть пустой";
                extensions["collectionName"] = ex.CollectionName;
                break;

            case FilmNotAvailableException ex:
                statusCode = HttpStatusCode.BadRequest;
                message = "Фильм недоступен для просмотра";
                extensions["filmId"] = ex.FilmId;
                break;

            case InvalidCodeException ex:
                statusCode = HttpStatusCode.BadRequest;
                message = "Неверный код доступа";
                extensions["roomId"] = ex.RoomId;
                break;

            case MaxNumberRoomsReachedException ex:
                statusCode = HttpStatusCode.BadRequest;
                message = "Достигнуто максимальное количество комнат";
                extensions["userId"] = ex.UserId;
                break;

            case RoomIsFullException ex:
                statusCode = HttpStatusCode.BadRequest;
                message = "Комната заполнена";
                extensions["roomId"] = ex.RoomId;
                break;

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