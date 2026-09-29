using System.Security.Claims;
using Identix.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Services;

namespace Identix.Application.Services.Services;

/// <summary>
/// Фабрика ClaimsIdentity для OpenID, которая обогащает данные пользователя и выстраивает правильные клаймы для Identity/Access токенов.
/// </summary>
/// <param name="userManager">Менеджер пользователей для работы с данными пользователей и их claims</param>
/// <param name="roleManager">Менеджер ролей для работы с ролевыми claims и разрешениями</param>
public class OpenIdClaimsIdentityFactory(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
  : IOpenIdClaimsIdentityFactory
{
  /// <summary>
  /// Обновляет существующий ClaimsPrincipal актуальными данными пользователя
  /// </summary>
  /// <param name="user">Пользователь с актуальными данными</param>
  /// <param name="scheme">Схема аутентификации</param>
  /// <param name="baseIdentity">Текущий identity для обновления</param>
  /// <returns>Обновленный ClaimsPrincipal</returns>
  public async Task<ClaimsIdentity> CreateAsync(AppUser user, string scheme, ClaimsIdentity baseIdentity)
  {
    ClaimsIdentity newIdentity = await BuildIdentityAsync(user, scheme);
    AddPreservedClaims(newIdentity, baseIdentity);

    return newIdentity;
  }

  /// <summary>
  /// Собирает Identity с базовыми и расширенными клаймами (username, email, phone, roles)
  /// </summary>
  /// <param name="user">Пользователь для построения identity</param>
  /// <param name="scheme">Схема аутентификации</param>
  /// <returns>ClaimsIdentity с claims пользователя</returns>
  private async Task<ClaimsIdentity> BuildIdentityAsync(AppUser user, string scheme)
  {
    var identity = new ClaimsIdentity(
      authenticationType: scheme,
      nameType: OpenIddictConstants.Claims.Name,
      roleType: OpenIddictConstants.Claims.Role);

    await AddSubjectClaimAsync(identity, user);
    await AddUsernameClaimsAsync(identity, user);

    if (userManager.SupportsUserEmail)
    {
      await AddEmailClaimsAsync(identity, user);
    }

    if (userManager.SupportsUserPhoneNumber)
    {
      await AddPhoneClaimsAsync(identity, user);
    }

    if (userManager.SupportsUserRole)
    {
      await AddRoleClaimsAsync(identity, user);
    }

    await AddUserClaimsAsync(identity, user);

    return identity;
  }

  /// <summary>
  /// Добавляет claims, связанные с идентификатором пользователя, в соответствии со стандартами OpenID Connect
  /// </summary>
  /// <param name="identity">ClaimsIdentity для добавления claims</param>
  /// <param name="user">Пользователь, для которого добавляются claims</param>
  private async Task AddSubjectClaimAsync(ClaimsIdentity identity, AppUser user)
  {
    string sub = await userManager.GetUserIdAsync(user);
    identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, sub));
  }

  /// <summary>
  /// Добавляет claims, связанные с именем пользователя, в соответствии со стандартами OpenID Connect
  /// </summary>
  /// <param name="identity">ClaimsIdentity для добавления claims</param>
  /// <param name="user">Пользователь, для которого добавляются claims</param>
  private async Task AddUsernameClaimsAsync(ClaimsIdentity identity, AppUser user)
  {
    string? username = await userManager.GetUserNameAsync(user);
    if (string.IsNullOrWhiteSpace(username)) return;

    identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, username));
    identity.AddClaim(new Claim(OpenIddictConstants.Claims.PreferredUsername, username));
  }

  /// <summary>
  /// Добавляет claims, связанные с ролями пользователя, в соответствии со стандартами OpenID Connect
  /// </summary>
  /// <param name="identity">ClaimsIdentity для добавления claims</param>
  /// <param name="user">Пользователь, для которого добавляются claims</param>
  private async Task AddRoleClaimsAsync(ClaimsIdentity identity, AppUser user)
  {
    IList<string> roles = await userManager.GetRolesAsync(user);

    foreach (string roleName in roles)
    {
      identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, roleName));
      if (!roleManager.SupportsRoleClaims) continue;

      AppRole? role = await roleManager.FindByNameAsync(roleName);
      if (role == null) continue;

      IList<Claim> roleClaims = await roleManager.GetClaimsAsync(role);
      identity.AddClaims(roleClaims.Where(c => _allowedClaims.Contains(c.Type)));
    }
  }

  /// <summary>
  /// Добавляет пользовательские claims из UserManager в соответствии с разрешенным списком claims
  /// </summary>
  /// <param name="identity">ClaimsIdentity для добавления claims</param>
  /// <param name="user">Пользователь, для которого добавляются claims</param>
  private async Task AddUserClaimsAsync(ClaimsIdentity identity, AppUser user)
  {
    IList<Claim> claims = await userManager.GetClaimsAsync(user);
    identity.AddClaims(claims.Where(c => _allowedClaims.Contains(c.Type)));
  }

  /// <summary>
  /// Добавляет claims, связанные с электронной почтой пользователя, в соответствии со стандартами OpenID Connect
  /// </summary>
  /// <param name="identity">ClaimsIdentity для добавления claims</param>
  /// <param name="user">Пользователь, для которого добавляются claims</param>
  private async Task AddEmailClaimsAsync(ClaimsIdentity identity, AppUser user)
  {
    string? email = await userManager.GetEmailAsync(user);
    if (string.IsNullOrWhiteSpace(email)) return;

    bool emailVerified = await userManager.IsEmailConfirmedAsync(user);
    identity.AddClaim(new Claim(OpenIddictConstants.Claims.Email, email));

    identity.AddClaim(new Claim(OpenIddictConstants.Claims.EmailVerified,
      emailVerified.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean));
  }

  /// <summary>
  /// Добавляет claims, связанные с телефонным номером пользователя, в соответствии со стандартами OpenID Connect
  /// </summary>
  /// <param name="identity">ClaimsIdentity для добавления claims</param>
  /// <param name="user">Пользователь, для которого добавляются claims</param>
  private async Task AddPhoneClaimsAsync(ClaimsIdentity identity, AppUser user)
  {
    string? phone = await userManager.GetPhoneNumberAsync(user);
    if (string.IsNullOrWhiteSpace(phone)) return;

    bool phoneVerified = await userManager.IsPhoneNumberConfirmedAsync(user);
    identity.AddClaim(new Claim(OpenIddictConstants.Claims.PhoneNumber, phone));

    identity.AddClaim(new Claim(OpenIddictConstants.Claims.PhoneNumberVerified,
      phoneVerified.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean));
  }

  /// <summary>
  /// Добавляет дополнительные claims, связанные с контекстом аутентификации, в соответствии со стандартами OpenID Connect
  /// </summary>
  /// <param name="identity">ClaimsIdentity для добавления claims</param>
  /// <param name="currentIdentity">Текущая ClaimsIdentity с исходными claims</param>
  private static void AddPreservedClaims(ClaimsIdentity identity, ClaimsIdentity currentIdentity)
  {
    IEnumerable<Claim> preservedClaims = currentIdentity.Claims
      .Where(c => c.Type is OpenIddictConstants.Claims.AuthenticationMethodReference
        or Constants.Claims.IdentityProvider
        or OpenIddictConstants.Claims.AuthenticationTime || c.Type.StartsWith("oi_"));

    identity.AddClaims(preservedClaims);
  }

  /// <summary>
  /// Список разрешенных claims (утверждений) для использования в системе.
  /// Включает стандартные claims OpenIddict и дополнительные кастомные claims.
  /// </summary>
  private static readonly HashSet<string> _allowedClaims =
  [
    .. typeof(OpenIddictConstants.Claims)

      .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)

      .Select(f => (string)f.GetValue(null)!),


    Constants.Claims.IdentityProvider

  ];

  /// <summary>
  /// Управляет попаданием клаймов в Access/Identity токены в зависимости от scope
  /// </summary>
  /// <param name="claim">Claim для которого определяется destination</param>
  /// <returns>Коллекция destinations куда может быть включен claim</returns>
  public IEnumerable<string> GetDestinations(Claim claim)
  {
    switch (claim.Type)
    {
      case OpenIddictConstants.Claims.Email or OpenIddictConstants.Claims.EmailVerified:
        if (claim.Subject!.HasScope(OpenIddictConstants.Scopes.Email))
          yield return OpenIddictConstants.Destinations.IdentityToken;
        yield break;

      case OpenIddictConstants.Claims.PhoneNumber or OpenIddictConstants.Claims.PhoneNumberVerified:
        if (claim.Subject!.HasScope(OpenIddictConstants.Scopes.Phone))
          yield return OpenIddictConstants.Destinations.IdentityToken;
        yield break;

      case OpenIddictConstants.Claims.Address or OpenIddictConstants.Claims.StreetAddress:
        if (claim.Subject!.HasScope(OpenIddictConstants.Scopes.Address))
          yield return OpenIddictConstants.Destinations.IdentityToken;
        yield break;

      case OpenIddictConstants.Claims.Role:
        yield return OpenIddictConstants.Destinations.AccessToken;
        if (claim.Subject!.HasScope(OpenIddictConstants.Scopes.Roles))
          yield return OpenIddictConstants.Destinations.IdentityToken;
        yield break;

      case Constants.Claims.IdentityProvider or OpenIddictConstants.Claims.AuthenticationMethodReference
        or OpenIddictConstants.Claims.AuthenticationTime:
        yield return OpenIddictConstants.Destinations.AccessToken;
        yield return OpenIddictConstants.Destinations.IdentityToken;
        yield break;

      default:
        if (claim.Subject!.HasScope(OpenIddictConstants.Scopes.Profile) && _allowedClaims.Contains(claim.Type))
          yield return OpenIddictConstants.Destinations.IdentityToken;
        yield break;
    }
  }
}