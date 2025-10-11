using MediatR;
using Rooms.Application.Abstractions.DTOs;

namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда на создание комнаты
/// </summary>
public class CreateRoomCommand : IRequest
{
    /// <summary>
    /// Идентификатор комнаты
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Зритель
    /// </summary>
    public required ViewerData Owner { get; init; }

    /// <summary> 
    /// Идентификатор фильма.
    /// </summary> 
    public required Guid FilmId { get; init; }

    /// <summary> 
    /// Тип фильма
    /// </summary> 
    public required bool IsSerial { get; init; }
}