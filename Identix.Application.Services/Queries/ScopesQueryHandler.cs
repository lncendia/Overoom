using System.Globalization;

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

    await foreach (object scope in scopeManager.FindByNamesAsync([..request.RequestedScopes], cancellationToken))
    {
      var descriptor = new OpenIddictScopeDescriptor();
      await scopeManager.PopulateAsync(descriptor, scope, cancellationToken);
      CultureInfo culture = request.Culture;
      string? displayName = descriptor.DisplayNames.TryGetValue(culture, out string? dn) ? dn : descriptor.DisplayName;
      string? description = descriptor.Descriptions.GetValueOrDefault(culture);

      results.Add(new ScopeDto
      {
        Name = descriptor.Name!,

        DisplayName = displayName ?? descriptor.Name!,

        Description = description,

        IdentityScope = descriptor.IsIdentityScope(),

        Emphasize = descriptor.GetEmphasize(),

        Required = descriptor.GetRequired(),

        Checked = request.RequestedScopes.Contains(descriptor.Name!, StringComparer.Ordinal)
      });
    }

    return results;
  }
}