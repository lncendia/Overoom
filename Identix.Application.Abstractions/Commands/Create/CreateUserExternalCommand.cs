using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identix.Application.Abstractions.Commands.Create;

/// <summary>
/// Команда для создания пользователя через внешний провайдер.
/// </summary>
public class CreateUserExternalCommand : IRequest<AppUser>
{
    /// <summary>
    /// Получает или задает информацию о внешней аутентификации.
    /// </summary>
    public required ExternalLoginInfo LoginInfo { get; init; }

    /// <summary>
    /// Получает или задает локализацию пользователя.
    /// </summary>
    public required Localization Locale { get; init; }
}