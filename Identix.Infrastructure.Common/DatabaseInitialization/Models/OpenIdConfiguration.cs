namespace Identix.Infrastructure.Common.DatabaseInitialization.Models;

/// <summary>Вспомогательная запись для десериализации конфигурации из JSON файла.</summary>
internal class OpenIdConfiguration
{
    /// <summary>Клиентские приложений</summary>
    public OpenIdApplication[] Applications { get; init; } = [];

    /// <summary>Области</summary>
    public OpenIdScope[] Scopes { get; init; } = [];
}