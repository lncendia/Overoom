using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace Common.DI.WebApi.Extensions;

///<summary>
/// Статический класс сервисов валидации.
///</summary>
public static class ValidationServices
{
  /// <summary>
  ///  Расширяющий метод для добавления сервисов валидации в коллекцию служб.
  /// </summary>
  /// <param name="services">Коллекция служб.</param>
  /// <param name="types">Типы, из зборок которых будут загружены валидаторы.</param>
  public static void AddValidationServices(this IServiceCollection services, params Type[] types)
  {
    services.AddValidatorsFromAssemblies(types.Select(t => t.Assembly));
    services.AddFluentValidationAutoValidation();
  }
}