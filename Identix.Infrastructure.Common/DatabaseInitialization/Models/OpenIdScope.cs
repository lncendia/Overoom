using System;
using System.Collections.Generic;
using System.Globalization;

namespace Identix.Infrastructure.Common.DatabaseInitialization.Models;

/// <summary>Область OpenID.</summary>
internal class OpenIdScope
{
    /// <summary>Описание области.</summary>
    public string? Description { get; init; }

    /// <summary>Локализованные описания.</summary>
    public Dictionary<CultureInfo, string> Descriptions { get; init; } = [];

    /// <summary>Отображаемое имя.</summary>
    public string? DisplayName { get; init; }
    
    /// <summary>Признак выделения области.</summary>
    public bool Emphasize { get; init; }

    /// <summary>Признак обязательности области.</summary>
    public bool Required { get; init; }
    
    /// <summary>Локализованные отображаемые имена.</summary>
    public Dictionary<CultureInfo, string> DisplayNames { get; init; } = [];

    /// <summary>Уникальное имя области видимости.</summary>
    public required string Name { get; init; }

    /// <summary>Ресурсы области видимости.</summary>
    public HashSet<string> Resources { get; init; } = new(StringComparer.Ordinal);
}