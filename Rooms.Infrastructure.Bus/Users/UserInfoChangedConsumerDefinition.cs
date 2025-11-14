using MassTransit;

namespace Rooms.Infrastructure.Bus.Users;

/// <summary>
/// Определение потребителя для события UserInfoChanged.
/// </summary>
public class UserInfoChangedConsumerDefinition : ConsumerDefinition<UserInfoChangedConsumer>
{
    public UserInfoChangedConsumerDefinition()
    {
        EndpointName = $"Rooms:{nameof(UserInfoChangedConsumer).Replace("Consumer", "")}";
    }

    /// <summary>
    /// Конфигурирует конечную точку и потребитель.
    /// </summary>
    /// <param name="endpointConfigurator">Конфигуратор конечной точки - для настройки поведения конечной точки</param>
    /// <param name="consumerConfigurator">Конфигуратор потребителя - для настройки поведения конкретного потребителя</param>
    /// <param name="context">Контекст регистрации - предоставляет доступ к зарегистрированным сервисам</param>
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<UserInfoChangedConsumer> consumerConfigurator, IRegistrationContext context)
    {
        // Настройка повторной обработки
        consumerConfigurator.UseMessageRetry(cfg => { cfg.Interval(5, TimeSpan.FromSeconds(5)); });
    }
}