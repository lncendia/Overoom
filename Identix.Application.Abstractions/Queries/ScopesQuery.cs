using System.Globalization;
using MediatR;

namespace Identix.Application.Abstractions.Queries;

/// <summary>
/// Запрос для получения детальной информации о запрашиваемых scope'ах с локализацией
/// </summary>
public class ScopesQuery : IRequest<IReadOnlyList<ScopeDto>>
{
    /// <summary>
    /// Список запрашиваемых scope'ов (разрешений) для получения детальной информации
    /// </summary>
    public required IReadOnlyList<string> RequestedScopes { get; init; }
    
    /// <summary>
    /// Культура (язык) для локализации отображаемых имен и описаний scope'ов
    /// </summary>
    public required CultureInfo Culture { get; init; }
}

/// <summary>
/// Модель scope'а (области видимости) с метаданными для отображения в UI
/// </summary>
public class ScopeDto
{
    /// <summary>
    /// Признак того, что scope относится к идентификационным данным пользователя
    /// </summary>
    public required bool IdentityScope { get; init; }
    
    /// <summary>
    /// Локализованное отображаемое имя scope'а
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Локализованное описание scope'а
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Уникальное техническое имя scope'а
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Признак необходимости визуального выделения scope'а в интерфейсе
    /// </summary>
    public required bool Emphasize { get; init; }

    /// <summary>
    /// Признак обязательности scope'а (не может быть отклонен пользователем)
    /// </summary>
    public required bool Required { get; init; }

    /// <summary>
    /// Состояние выбора scope'а в интерфейсе (выбран/не выбран)
    /// </summary>
    public required bool Checked { get; set; }
}