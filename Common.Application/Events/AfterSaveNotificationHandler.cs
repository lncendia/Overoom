using Common.Domain.Events;
using MediatR;

namespace Common.Application.Events;

/// <summary>
/// Базовый класс для обработчиков событий, которые должны выполняться
/// только в фазе "после сохранения" (BeforeSave = false).
/// </summary>
/// <typeparam name="T">Тип события</typeparam>
public abstract class AfterSaveNotificationHandler<T> : INotificationHandler<T>
    where T : DomainEvent
{
    public Task Handle(T notification, CancellationToken cancellationToken)
    {
        return notification.BeforeSave
            ? Task.CompletedTask
            : Execute(notification, cancellationToken);
    }

    /// <summary>
    /// Логика, выполняемая только после сохранения в БД.
    /// </summary>
    protected abstract Task Execute(T notification, CancellationToken cancellationToken);
}