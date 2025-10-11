using MediatR;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Extensions;
using Identix.Application.Abstractions.Queries;

namespace Identix.Application.Services.Queries;

/// <summary>
/// Обработчик запроса для получения детальной информации о scope'ах с локализацией
/// </summary>
public class ScopesQueryHandler(IOpenIddictScopeManager scopeManager)
    : IRequestHandler<ScopesQuery, IReadOnlyList<ScopeDto>>
{
    /// <summary>
    /// Обрабатывает запрос на получение информации о scope'ах
    /// </summary>
    /// <param name="request">Запрос со списком scope'ов и языком локализации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список DTO объектов с информацией о scope'ах</returns>
    public async Task<IReadOnlyList<ScopeDto>> Handle(ScopesQuery request, CancellationToken cancellationToken)
    {
        var results = new List<ScopeDto>();

        // Ищем scope'ы по именам в хранилище OpenIddict
        await foreach (var scope in scopeManager.FindByNamesAsync([..request.RequestedScopes], cancellationToken))
        {
            // Создаем и заполняем дескриптор scope для получения метаданных
            var descriptor = new OpenIddictScopeDescriptor();
            await scopeManager.PopulateAsync(descriptor, scope, cancellationToken);

            // Получаем локализованные значения для указанной культуры
            var culture = request.Culture;
            var displayName = descriptor.DisplayNames.TryGetValue(culture, out var dn) ? dn : descriptor.DisplayName;
            var description = descriptor.Descriptions.GetValueOrDefault(culture);

            // Формируем DTO объекта scope
            results.Add(new ScopeDto
            {
                // Уникальное техническое имя scope'а
                Name = descriptor.Name!,
                
                // Локализованное отображаемое имя (или техническое, если локализация не найдена)
                DisplayName = displayName ?? descriptor.Name!,
                
                // Локализованное описание scope'а
                Description = description,
                
                // Признак identity scope'а (доступ к данным пользователя)
                IdentityScope = descriptor.IsIdentityScope(),
                
                // Признак визуального выделения в UI
                Emphasize = descriptor.GetEmphasize(),
                
                // Признак обязательности scope'а
                Required = descriptor.GetRequired(),
                
                // Scope включен по умолчанию (так как присутствует в запросе)
                Checked = request.RequestedScopes.Contains(descriptor.Name!, StringComparer.Ordinal)
            });
        }

        return results;
    }
}