using Identix.Application.Abstractions.Enums;
using MediatR;

namespace Identix.Application.Abstractions.Commands.Profile;

/// <summary>
/// Команда для изменения локали пользователя.
/// </summary>
public class ChangeLocaleCommand : IRequest
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Локализация пользователя.
    /// </summary>
    public required Localization Localization { get; init; }
}