using System.ComponentModel.DataAnnotations;

namespace Identix.Infrastructure.Web.Settings.InputModels;

/// <summary>
/// Модель запроса отключения провайдера аутентификации
/// </summary>
public class RemoveLoginInputModel
{
    /// <summary>
    /// Провайдер, логин которого нужно удалить
    /// </summary>
    [Required]
    public string? Provider { get; set; }

    /// <summary>
    /// URL для возврата после операции
    /// </summary>
    public string ReturnUrl { get; set; } = "/";
}