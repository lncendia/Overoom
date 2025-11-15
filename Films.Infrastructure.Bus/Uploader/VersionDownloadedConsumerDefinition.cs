using Films.Application.Abstractions.Exceptions;
using MassTransit;

namespace Films.Infrastructure.Bus.Uploader;

/// <summary>
/// Определение потребителя для события VersionDownloaded.
/// </summary>
public class VersionDownloadedConsumerDefinition : ConsumerDefinition<VersionDownloadedConsumer>
{
    /// <summary>
    /// Конфигурирует конечную точку и потребитель.
    /// </summary>
    /// <param name="endpointConfigurator">Конфигуратор конечной точки - для настройки поведения конечной точки</param>
    /// <param name="consumerConfigurator">Конфигуратор потребителя - для настройки поведения конкретного потребителя</param>
    /// <param name="context">Контекст регистрации - предоставляет доступ к зарегистрированным сервисам</param>
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<VersionDownloadedConsumer> consumerConfigurator, IRegistrationContext context)
    {
        // Настройка повторной обработки
        consumerConfigurator.UseMessageRetry(cfg =>
        {
            cfg.Interval(5, TimeSpan.FromSeconds(5));
            cfg.Ignore<FilmNotFoundException>();
        });

        // Настройка отложенной повторной доставки с экспоненциальной политикой
        endpointConfigurator.UseScheduledRedelivery(cfg =>
        {
            cfg.Exponential(10, TimeSpan.FromMinutes(1), TimeSpan.FromDays(1), TimeSpan.FromSeconds(30));
            cfg.Ignore<FilmNotFoundException>();
        });
    }
}