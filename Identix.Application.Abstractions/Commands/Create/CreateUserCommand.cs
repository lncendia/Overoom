using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Enums;
using MediatR;

namespace Identix.Application.Abstractions.Commands.Create;

/// <summary>
/// Команда для создания пользователя.
/// </summary>
public class CreateUserCommand : IRequest<AppUser>
{
    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Электронная почта пользователя.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// URL для подтверждения пользователя.
    /// </summary>
    public required string ConfirmUrl { get; init; }
    
    /// <summary>
    /// URL адрес возврата
    /// </summary>
    public string? ReturnUrl { get; init; }

    /// <summary>
    /// Локаль пользователя.
    /// </summary>
    public required Localization Locale { get; init; }
}