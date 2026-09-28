using System.Collections.Immutable;

using MediatR;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Extensions;
using Identix.Application.Abstractions.Queries;

namespace Identix.Application.Services.Queries;

/// <summary>
/// Обработчик запроса на получение списка грантов (разрешений) пользователя
/// </summary>
public class UserGrantsHandler(
  IOpenIddictApplicationManager applicationManager,
  IOpenIddictAuthorizationManager authorizationManager,
  IOpenIddictScopeManager scopeManager)
  : IRequestHandler<UserGrantsQuery, IReadOnlyList<GrantDto>>
{
  /// <summary>
  /// Обрабатывает запрос на получение грантов пользователя
  /// </summary>
  /// <param name="request">Запрос с параметрами пользователя</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Список DTO объектов с информацией о грантах</returns>
  public async Task<IReadOnlyList<GrantDto>> Handle(UserGrantsQuery request, CancellationToken cancellationToken)
  {
    List<object> authorizations = await authorizationManager.FindAsync(
      subject: request.UserId.ToString(),
      client: null,
      status: OpenIddictConstants.Statuses.Valid,
      type: OpenIddictConstants.AuthorizationTypes.Permanent,
      scopes: null,
      cancellationToken: cancellationToken
    ).ToListAsync(cancellationToken);

    var grants = new List<GrantDto>();

    foreach (object authorization in authorizations)
    {
      var authDescriptor = new OpenIddictAuthorizationDescriptor();
      await authorizationManager.PopulateAsync(authDescriptor, authorization, cancellationToken);
      object? app = await applicationManager.FindByIdAsync(authDescriptor.ApplicationId!, cancellationToken);
      if (app is null) continue;

      var appDescriptor = new OpenIddictApplicationDescriptor();
      await applicationManager.PopulateAsync(appDescriptor, app, cancellationToken);
      ImmutableArray<string> scopeNames = await authorizationManager.GetScopesAsync(authorization, cancellationToken);
      var scopes = new List<GrantScopeDto>(scopeNames.Length);

      await foreach (object scope in scopeManager.FindByNamesAsync(scopeNames, cancellationToken))
      {
        var descriptor = new OpenIddictScopeDescriptor();
        await scopeManager.PopulateAsync(descriptor, scope, cancellationToken);
        bool isIdentity = descriptor.IsIdentityScope();
        string? displayName = descriptor.GetDisplayName(request.Culture);
        string? description = descriptor.GetDescription(request.Culture);

        scopes.Add(new GrantScopeDto
        {
          IdentityScope = isIdentity,
          DisplayName = displayName ?? descriptor.Name ?? string.Empty,
          Description = description
        });
      }

      grants.Add(new GrantDto
      {
        Id = await authorizationManager.GetIdAsync(authorization, cancellationToken) ?? string.Empty,

        ApplicationId = authDescriptor.ApplicationId ?? string.Empty,

        ClientName = appDescriptor.DisplayName ?? authDescriptor.ApplicationId!,

        ClientUrl = appDescriptor.GetClientUrl(),

        ClientLogoKey = appDescriptor.GetLogoKey(),

        Description = authDescriptor.GetDescription(),

        Created = authDescriptor.CreationDate?.ToUniversalTime().Date ?? DateTime.MinValue,

        Scopes = scopes
      });
    }

    return grants;
  }
}