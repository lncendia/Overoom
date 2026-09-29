using Identix.Application.Abstractions.Entities;
using MediatR;

namespace Identix.Application.Abstractions.Commands.Authentication;

/// <summary>
/// Команда на закрытие остальных сессий пользователя
/// </summary>
public class UpdateSecurityStampCommand : IRequest<AppUser>
{
  /// <summary>
  /// Идентификатор пользователя.
  /// </summary>
  public required Guid UserId { get; init; }
}
