using System.Security.Claims;

using Identix.Application.Abstractions.Enums;

using OpenIddict.Abstractions;

using Identix.Application.Abstractions.Extensions;

using Microsoft.AspNetCore.Identity;

namespace Identix.Application.Abstractions.Entities;

/// <summary>
/// Класс, представляющий пользователя приложения.
/// </summary>
public sealed class AppUser : IdentityUser<Guid>
{
  public AppUser()
  {
    Id = Guid.NewGuid();
  }

  /// <summary>
  /// Дата и время регистрации пользователя.
  /// Это обязательное свойство, которое указывает, когда пользователь зарегистрировался в системе.
  /// </summary>
  public required DateTime RegistrationTimeUtc { get; init; }

  /// <summary>
  /// Дата и время последней аутентификации пользователя.
  /// Это обязательное свойство, которое указывает, когда пользователь последний раз входил в систему.
  /// </summary>
  public required DateTime LastAuthTimeUtc { get; set; }

  /// <summary>
  /// Локализация пользователя, определяемая на основе его настроек.
  /// </summary>
  public static Localization GetLocale(IEnumerable<Claim> claims)
  {
    string? claim = claims.FirstOrDefault(c => c.Type == OpenIddictConstants.Claims.Locale)?.Value;

    return claim.GetLocalization();
  }

  /// <summary>
  /// URI аватара пользователя (миниатюра).
  /// </summary>
  public static string? GetPhotoKey(IEnumerable<Claim> claims)
  {
    return claims.FirstOrDefault(c => c.Type == OpenIddictConstants.Claims.Picture)?.Value;
  }
}
