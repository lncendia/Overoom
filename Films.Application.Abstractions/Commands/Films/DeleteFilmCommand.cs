using MediatR;

namespace Films.Application.Abstractions.Commands.Films;

/// <summary>
/// Команда для удаления фильма
/// </summary>
public class DeleteFilmCommand : IRequest
{
    /// <summary>
    /// Идентификатор фильма
    /// </summary>
    public required Guid Id { get; init; }
}