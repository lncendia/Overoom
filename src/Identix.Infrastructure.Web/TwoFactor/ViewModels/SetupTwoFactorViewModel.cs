using System.Text;
using System.Web;
using Identix.Infrastructure.Web.TwoFactor.InputModels;

namespace Identix.Infrastructure.Web.TwoFactor.ViewModels;

/// <summary>
/// Класс модели представления для настройки двухфакторной аутентификации.
/// </summary>
public class SetupTwoFactorViewModel : SetupTwoFactorInputModel
{
  /// <summary>
  /// Формат URI для аутентификатора.
  /// </summary>
  private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

  /// <summary>
  /// URL QR-кода.
  /// </summary>
  public string QrCodeUrl { get; }

  /// <summary>
  /// Конструктор класса SetupTwoFactorViewModel.
  /// </summary>
  /// <param name="authenticatorKey">Ключ аутентификатора.</param>
  /// <param name="name">Имя пользователя.</param>
  /// <param name="projectName">Название проекта.</param>
  public SetupTwoFactorViewModel(string authenticatorKey, string name, string projectName)
  {
    AuthenticatorKey = FormatKey(authenticatorKey);
    QrCodeUrl = GenerateQrCodeUrl(name, projectName, authenticatorKey);
  }

  /// <summary>
  /// Форматирует ключ.
  /// </summary>
  private static string FormatKey(string unformattedKey)
  {
    var result = new StringBuilder();
    int currentPosition = 0;

    while (currentPosition + 4 < unformattedKey.Length)
    {
      result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
      currentPosition += 4;
    }

    if (currentPosition < unformattedKey.Length)
      result.Append(unformattedKey[currentPosition..]);

    return result.ToString().ToLowerInvariant();
  }

  /// <summary>
  /// Генерирует URL QR-кода.
  /// </summary>
  private static string GenerateQrCodeUrl(string name, string projectName, string unformattedKey)
  {
    return string.Format(AuthenticatorUriFormat, HttpUtility.UrlEncode(projectName),
      HttpUtility.UrlEncode(name), unformattedKey);
  }
}