using MediatR;

namespace Common.Domain.Events;

/// <summary>
/// Базовый класс для доменных событий
/// </summary>
public abstract class DomainEvent : INotification
{
    /// <summary>
    /// Флаг, указывающий должно ли событие обрабатываться до сохранения
    /// </summary>
    public bool BeforeSave { get; set; }
}