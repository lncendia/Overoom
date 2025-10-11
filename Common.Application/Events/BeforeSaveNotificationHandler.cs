using Common.Domain.Events;
using MediatR;

namespace Common.Application.Events;

/// <summary>
/// Базовый класс для обработчиков событий, которые должны выполняться
/// только в фазе "до сохранения" (BeforeSave = true).
/// </summary>
/// <typeparam name="T">Тип события</typeparam>
public abstract class BeforeSaveNotificationHandler<T> : INotificationHandler<T>
    where T : DomainEvent
{
    public Task Handle(T notification, CancellationToken cancellationToken)
    {
        return notification.BeforeSave
            ? Execute(notification, cancellationToken)
            : Task.CompletedTask;
    }

    /// <summary>
    /// Логика, выполняемая только до сохранения в БД.
    /// </summary>
    protected abstract Task Execute(T notification, CancellationToken cancellationToken);
}