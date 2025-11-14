using Identix.Application.Abstractions.Entities;
using MediatR;

namespace Identix.Application.Abstractions.Queries;

/// <summary>
/// Запрос для получения пользователя по идентификатору.
/// </summary>
public class UserByIdQuery : IRequest<AppUser>
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public required Guid Id { get; init; }
}