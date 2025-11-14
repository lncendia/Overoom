using MediatR;

namespace Identix.Application.Abstractions.Queries;

/// <summary>
/// Запрос для получения списка внешних аутентификаций пользователя.
/// </summary>
public class UserLoginsQuery : IRequest<IReadOnlyCollection<string>>
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public required Guid Id { get; init; }
}