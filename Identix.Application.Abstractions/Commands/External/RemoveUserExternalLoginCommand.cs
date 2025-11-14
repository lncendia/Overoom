using Identix.Application.Abstractions.Entities;
using MediatR;

namespace Identix.Application.Abstractions.Commands.External;

/// <summary>
/// Команда для удаления внешней аутентификации пользователя.
/// </summary>
public class RemoveUserExternalLoginCommand : IRequest<AppUser>
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Провайдер внешней аутентификации.
    /// </summary>
    public required string Provider { get; init; }
}