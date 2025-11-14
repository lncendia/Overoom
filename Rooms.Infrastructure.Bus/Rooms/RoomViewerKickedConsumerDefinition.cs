using MassTransit;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Rooms.Exceptions;

namespace Rooms.Infrastructure.Bus.Rooms;

/// <summary>
/// Определение потребителя для события RoomViewerKicked.
/// </summary>
public class RoomViewerKickedConsumerDefinition : ConsumerDefinition<RoomViewerKickedConsumer>
{
    /// <summary>
    /// Конфигурирует конечную точку и потребитель.
    /// </summary>
    /// <param name="endpointConfigurator">Конфигуратор конечной точки - для настройки поведения конечной точки</param>
    /// <param name="consumerConfigurator">Конфигуратор потребителя - для настройки поведения конкретного потребителя</param>
    /// <param name="context">Контекст регистрации - предоставляет доступ к зарегистрированным сервисам</param>
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<RoomViewerKickedConsumer> consumerConfigurator, IRegistrationContext context)
    {
        // Настройка повторной обработки
        consumerConfigurator.UseMessageRetry(cfg =>
        {
            cfg.Interval(5, TimeSpan.FromSeconds(5));
            cfg.Ignore<RoomNotFoundException>();
            cfg.Ignore<ViewerNotFoundException>();
        });
    }
}