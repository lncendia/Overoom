namespace Rooms.Application.Abstractions.DTOs;

/// <summary>
/// DTO для передачи данных тега зрителя
/// </summary>
public class ViewerTagDto
{
  /// <summary>
  /// Название тега
  /// </summary>
  public required string Name { get; init; }

  /// <summary>
  /// Описание тега
  /// </summary>
  public string? Description { get; init; }
}
