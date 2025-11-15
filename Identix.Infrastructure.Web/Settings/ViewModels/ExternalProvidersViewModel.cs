using Identix.Infrastructure.Web.Settings.InputModels;

namespace Identix.Infrastructure.Web.Settings.ViewModels;

/// <summary>
/// Модель представления настроек внешней аутентификации.
/// </summary>
public class ExternalProvidersViewModel : RemoveLoginInputModel
{
    /// <summary>
    /// Внешние поставщики
    /// </summary>
    public required IEnumerable<ExternalProvider> ExternalProviders { get; init; }
}