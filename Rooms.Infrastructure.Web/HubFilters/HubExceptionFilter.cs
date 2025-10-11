using Microsoft.AspNetCore.SignalR;
using Rooms.Application.Abstractions.Events.Notifications;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Rooms.Exceptions;
using Rooms.Infrastructure.Web.Rooms.Exceptions;

namespace Rooms.Infrastructure.Web.HubFilters;

/// <summary>
/// Фильтр для централизованной обработки исключений в SignalR хабах
/// </summary>
/// <remarks>
/// Перехватывает все исключения, возникающие при вызове методов хаба,
/// преобразует их в понятные клиенту сообщения об ошибках и отправляет
/// через соединение SignalR
/// </remarks>
public class HubExceptionFilter : IHubFilter
{
    /// <summary>
    /// Обрабатывает вызов методов хаба
    /// </summary>
    /// <param name="invocationContext">Контекст вызова метода хаба</param>
    /// <param name="next">Делегат для вызова следующего фильтра или метода хаба</param>
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            // Выполняем следующий фильтр или метод хаба
            return await next(invocationContext);
        }
        catch (Exception ex)
        {
            // Обрабатываем исключение и отправляем сообщение клиенту
            await HandleHubException(invocationContext.Hub, ex);

            // Пробрасываем исключение дальше
            throw;
        }
    }

    /// <summary>
    /// Преобразует исключение в сообщение об ошибке и отправляет клиенту
    /// </summary>
    /// <param name="hub">Экземпляр хаба, где произошло исключение</param>
    /// <param name="ex">Исключение, которое нужно обработать</param>
    /// <returns>Task, представляющий асинхронную операцию отправки сообщения</returns>
    /// <remarks>
    /// Содержит логику преобразования различных типов исключений в пользовательские сообщения
    /// </remarks>
    private static Task HandleHubException(Hub hub, Exception ex)
    {
        // Преобразуем исключение в сообщение об ошибке
        var error = ex switch
        {
            RoomNotFoundException => "Комната не найдена",
            ViewerNotFoundException => "Зритель не найден",
            ActionNotAllowedException => "Действие не разрешено",
            ChangeFilmSeriesException => "Вы не можете изменить серию у фильма",
            ActionCooldownException a => $"Действие будет доступно через {FormatActionCooldown(a.Seconds)}",
            ArgumentException => "Данные указаны некорректно или в неверном формате",
            _ => "Внутренняя ошибка"
        };

        // Отправляем сообщение об ошибке вызывающему клиенту
        return hub.Clients.Caller.SendAsync("Event", new ErrorNotificationEvent { Message = error });
    }
    
    /// <summary>
    /// Формирует текст уведомления о задержке действия с корректным склонением слова "секунда".
    /// </summary>
    /// <param name="seconds">Количество секунд до доступности действия.</param>
    /// <returns>Строка вида "Действие будет доступно через N секунду/секунды/секунд".</returns>
    private static string FormatActionCooldown(int seconds)
    {
        string suffix;

        var lastDigit = seconds % 10;
        var lastTwoDigits = seconds % 100;

        if (lastTwoDigits is >= 11 and <= 14)
            suffix = "секунд";
        else
            suffix = lastDigit switch
            {
                1 => "секунду",
                >= 2 and <= 4 => "секунды",
                _ => "секунд"
            };

        return $"{seconds} {suffix}";
    }

}