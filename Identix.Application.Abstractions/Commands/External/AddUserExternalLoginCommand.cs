using Identix.Application.Abstractions.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identix.Application.Abstractions.Commands.External;

/// <summary>
/// Команда для добавления внешней аутентификации пользователю.
/// </summary>
public class AddUserExternalLoginCommand : IRequest<AppUser>
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Информация о внешней аутентификации.
    /// </summary>
    public required ExternalLoginInfo LoginInfo { get; init; }
}