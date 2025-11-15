using MediatR;

namespace Identix.Application.Abstractions.Queries;

/// <summary>
/// Запрос для получения информации о клиентском приложении по его идентификатору
/// </summary>
public class ClientQuery : IRequest<ClientDto>
{
    /// <summary>
    /// Идентификатор клиентского приложения (client_id)
    /// </summary>
    public required string ClientId { get; init; }
}

/// <summary>
/// Data Transfer Object с информацией о клиентском приложении
/// </summary>
public class ClientDto
{
    /// <summary>
    /// Отображаемое имя клиентского приложения
    /// </summary>
    public required string ClientName { get; init; }

    /// <summary>
    /// URL веб-сайта клиентского приложения
    /// </summary>
    public string? ClientUrl { get; init; }

    /// <summary>
    /// Ключ логотипа клиентского приложения
    /// </summary>
    public string? ClientLogoKey { get; init; }
}