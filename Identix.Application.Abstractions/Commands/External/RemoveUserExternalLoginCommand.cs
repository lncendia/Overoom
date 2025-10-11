using Identix.Application.Abstractions.Entities;
using MediatR;

namespace Identix.Application.Abstractions.Commands.External;

/// <summary>
/// Команда для удаления внешней аутентификации пользователя.
/// </summary>
public class RemoveUserExternalLoginCommand : IRequest<AppUser>
{
    /// <summary>
    /// Получает или задает идентификатор пользователя.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Получает или задает провайдер внешней аутентификации.
    /// </summary>
    public required string Provider { get; init; }
}