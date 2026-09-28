using Microsoft.Extensions.DependencyInjection;

namespace Common.DI.WebApi.Extensions;

///<summary>
/// Статический класс сервисов мапинга.
///</summary>
public static class MappingServices
{
  /// <summary>
  ///  Расширяющий метод для добавления сервисов мапинга в коллекцию служб.
  /// </summary>
  /// <param name="services">Коллекция служб.</param>
  /// <param name="types">Типы, из сборки которых будут созданы карты.</param>
  public static void AddMappingServices(this IServiceCollection services, params Type[] types)
  {
    services.AddAutoMapper(cfg =>
    {
      cfg.AddMaps(types.Select(t => t.Assembly));
    });
  }
}