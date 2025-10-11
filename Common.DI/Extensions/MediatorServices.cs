using Microsoft.Extensions.DependencyInjection;

namespace Common.DI.Extensions;

///<summary>
/// Статический класс сервисов Mediator.
///</summary>
public static class MediatorServices
{
    /// <summary>
    ///  Расширяющий метод для добавления сервисов Mediator в коллекцию служб.
    /// </summary>
    /// <param name="services">Коллекция служб.</param>
    /// <param name="types">Типы, из сборок которых будут загружены обработчики.</param>
    public static void AddMediatorServices(this IServiceCollection services, params Type[] types)
    {
        // Регистрация сервисов MediatR и обработчиков команд
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblies(types.Select(t => t.Assembly).ToArray());
        });
    }
}