using MassTransit;

namespace Rooms.Infrastructure.Bus.Rooms;

/// <summary>
/// Определение потребителя для события CleanMessages.
/// </summary>
public class CleanMessagesConsumerDefinition : ConsumerDefinition<CleanMessagesConsumer>
{
    /// <summary>
    /// Конфигурирует конечную точку и потребитель.
    /// </summary>
    /// <param name="endpointConfigurator">Конфигуратор конечной точки - для настройки поведения конечной точки</param>
    /// <param name="consumerConfigurator">Конфигуратор потребителя - для настройки поведения конкретного потребителя</param>
    /// <param name="context">Контекст регистрации - предоставляет доступ к зарегистрированным сервисам</param>
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<CleanMessagesConsumer> consumerConfigurator, IRegistrationContext context)
    {
        // Настройка отложенной повторной доставки с экспоненциальной политикой
        endpointConfigurator.UseScheduledRedelivery(cfg =>
        {
            cfg.Exponential(10, TimeSpan.FromMinutes(1), TimeSpan.FromDays(5), TimeSpan.FromSeconds(30));
        });
    }
}