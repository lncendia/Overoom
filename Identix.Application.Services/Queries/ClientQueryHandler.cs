using MediatR;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Extensions;
using Identix.Application.Abstractions.Queries;

namespace Identix.Application.Services.Queries;

/// <summary>
/// Обработчик запроса для получения информации о клиентском приложении
/// </summary>
public class ClientQueryHandler(IOpenIddictApplicationManager applicationManager)
  : IRequestHandler<ClientQuery, ClientDto>
{
  /// <summary>
  /// Обрабатывает запрос на получение информации о клиентском приложении
  /// </summary>
  /// <param name="request">Запрос с идентификатором клиента</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>DTO с информацией о клиентском приложении</returns>
  /// <exception cref="ArgumentException">Если приложение с указанным client_id не найдено</exception>
  public async Task<ClientDto> Handle(ClientQuery request, CancellationToken cancellationToken)
  {
    object? application = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);

    if (application is null)
      throw new ArgumentException($"Client application with ID '{request.ClientId}' not found");

    var descriptor = new OpenIddictApplicationDescriptor();
    await applicationManager.PopulateAsync(descriptor, application, cancellationToken);

    return new ClientDto
    {
      ClientName = descriptor.DisplayName ?? descriptor.ClientId ?? string.Empty,

      ClientUrl = descriptor.GetClientUrl(),

      ClientLogoKey = descriptor.GetLogoKey()
    };
  }
}