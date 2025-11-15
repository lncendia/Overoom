using MediatR;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Extensions;
using Identix.Application.Abstractions.Queries;

namespace Identix.Application.Services.Queries;

/// <summary>
/// Обработчик запроса для получения информации о клиентском приложении
/// </summary>
public class ClientQueryHandler(IOpenIddictApplicationManager applicationManager) : IRequestHandler<ClientQuery, ClientDto>
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
        // Ищем приложение по client_id в хранилище OpenIddict
        var application = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        
        // Если приложение не найдено, выбрасываем исключение
        if (application is null) 
            throw new ArgumentException($"Client application with ID '{request.ClientId}' not found");

        // Создаем и заполняем дескриптор приложения для получения метаданных
        var descriptor = new OpenIddictApplicationDescriptor();
        await applicationManager.PopulateAsync(descriptor, application, cancellationToken);

        // Формируем и возвращаем DTO с информацией о приложении
        return new ClientDto
        {
            // Используем отображаемое имя или client_id, если имя не задано
            ClientName = descriptor.DisplayName ?? descriptor.ClientId ?? string.Empty,
            
            // Получаем URL веб-сайта приложения
            ClientUrl = descriptor.GetClientUrl(),
            
            // Получаем URL логотипа приложения
            ClientLogoKey = descriptor.GetLogoKey()
        };
    }
}