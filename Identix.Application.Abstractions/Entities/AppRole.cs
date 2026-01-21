using Microsoft.AspNetCore.Identity;

namespace Identix.Application.Abstractions.Entities;

/// <summary>
/// Класс, представляющий роль в приложении.
/// </summary>
public sealed class AppRole : IdentityRole<Guid>
{
  public AppRole()
  {
    Id = Guid.NewGuid();
  }

  /// <summary>
  /// Описание роли.
  /// Это необязательное свойство, которое предоставляет дополнительную информацию о роли,
  /// например, её назначение или область применения.
  /// </summary>
  public string? Description { get; set; }
}
