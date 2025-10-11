using AspNetCore.Identity.Mongo.Model;

namespace Identix.Application.Abstractions.Entities;

/// <summary>
/// Класс, представляющий роль в приложении.
/// Наследуется от <see cref="MongoRole{Guid}"/>, что обеспечивает базовую функциональность роли,
/// такую как хранение идентификатора, имени роли и связанных с ней разрешений.
/// </summary>
public class AppRole : MongoRole<Guid>
{
    /// <summary>
    /// Описание роли.
    /// Это необязательное свойство, которое предоставляет дополнительную информацию о роли,
    /// например, её назначение или область применения.
    /// </summary>
    public string? Description { get; set; }
}