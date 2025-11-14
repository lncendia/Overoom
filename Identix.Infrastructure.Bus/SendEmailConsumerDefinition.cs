using MassTransit;

namespace Identix.Infrastructure.Bus;

/// <summary>
/// Определение потребителя для события SendEmailConsumer.
/// </summary>
public class SendEmailConsumerDefinition : ConsumerDefinition<SendEmailConsumer>
{
    /// <summary>
    /// Конфигурирует конечную точку и потребитель.
    /// </summary>
    /// <param name="endpointConfigurator">Конфигуратор конечной точки - для настройки поведения конечной точки</param>
    /// <param name="consumerConfigurator">Конфигуратор потребителя - для настройки поведения конкретного потребителя</param>
    /// <param name="context">Контекст регистрации - предоставляет доступ к зарегистрированным сервисам</param>
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<SendEmailConsumer> consumerConfigurator, IRegistrationContext context)
    {
        // Настройка повторной обработки
        consumerConfigurator.UseMessageRetry(cfg =>
        {
            cfg.Interval(5, TimeSpan.FromSeconds(30));
        });
        
        // Настройка отложенной повторной доставки с экспоненциальной политикой
        consumerConfigurator.UseScheduledRedelivery(cfg =>
        {
            cfg.Exponential(10, TimeSpan.FromMinutes(1), TimeSpan.FromHours(5), TimeSpan.FromSeconds(30));
        });
    }
}