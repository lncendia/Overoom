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
        // Создаем новую identity с актуальными данными пользователя
        var newIdentity = await BuildIdentityAsync(user, scheme);

        // Сохраняем важные claims аутентификации из текущего principal
        AddPreservedClaims(newIdentity, baseIdentity);

        // Возвращаем готовую identity для использования в токенах
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
        // Создаём пустую identity с правильными типами name/role для OpenID Connect
        var identity = new ClaimsIdentity(
            authenticationType: scheme,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        // Добавляем основной идентификатор пользователя (claim 'sub')
        await AddSubjectClaimAsync(identity, user);

        // Добавляем claims связанные с именем пользователя (name, preferred_username)
        await AddUsernameClaimsAsync(identity, user);

        // Добавляем claims email если UserManager поддерживает работу с email
        if (userManager.SupportsUserEmail)
        {
            await AddEmailClaimsAsync(identity, user);
        }

        // Добавляем claims телефона если UserManager поддерживает работу с номерами телефонов
        if (userManager.SupportsUserPhoneNumber)
        {
            await AddPhoneClaimsAsync(identity, user);
        }

        // Добавляем claims ролей если UserManager поддерживает систему ролей
        if (userManager.SupportsUserRole)
        {
            await AddRoleClaimsAsync(identity, user);
        }

        // Добавляем кастомные пользовательские claims из хранилища
        await AddUserClaimsAsync(identity, user);

        // Возвращаем полностью сформированную identity со всеми claims пользователя
        return identity;
    }

    /// <summary>
    /// Добавляет claims, связанные с идентификатором пользователя, в соответствии со стандартами OpenID Connect
    /// </summary>
    /// <param name="identity">ClaimsIdentity для добавления claims</param>
    /// <param name="user">Пользователь, для которого добавляются claims</param>
    private async Task AddSubjectClaimAsync(ClaimsIdentity identity, AppUser user)
    {
        // Получаем уникальный идентификатор пользователя из UserManager
        var sub = await userManager.GetUserIdAsync(user);

        // Добавляем OIDC claim 'sub'
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, sub));
    }

    /// <summary>
    /// Добавляет claims, связанные с именем пользователя, в соответствии со стандартами OpenID Connect
    /// </summary>
    /// <param name="identity">ClaimsIdentity для добавления claims</param>
    /// <param name="user">Пользователь, для которого добавляются claims</param>
    private async Task AddUsernameClaimsAsync(ClaimsIdentity identity, AppUser user)
    {
        // Получаем имя пользователя из UserManager
        var username = await userManager.GetUserNameAsync(user);
        if (string.IsNullOrWhiteSpace(username)) return;

        // Добавляем OIDC claim 'name'
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, username));

        // Добавляем OIDC claim 'preferred_username'
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.PreferredUsername, username));
    }

    /// <summary>
    /// Добавляет claims, связанные с ролями пользователя, в соответствии со стандартами OpenID Connect
    /// </summary>
    /// <param name="identity">ClaimsIdentity для добавления claims</param>
    /// <param name="user">Пользователь, для которого добавляются claims</param>
    private async Task AddRoleClaimsAsync(ClaimsIdentity identity, AppUser user)
    {
        // Получаем список ролей пользователя из UserManager
        var roles = await userManager.GetRolesAsync(user);

        // Добавляем claim для каждой роли пользователя
        foreach (var roleName in roles)
        {
            // Добавляем основную claim роли
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, roleName));

            // Если RoleManager поддерживает claims ролей, добавляем дополнительные claims
            if (!roleManager.SupportsRoleClaims) continue;

            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            // Получаем claims, связанные с ролью, и фильтруем по разрешенному списку
            var roleClaims = await roleManager.GetClaimsAsync(role);
            identity.AddClaims(roleClaims.Where(c => AllowedClaims.Contains(c.Type)));
        }
    }

    /// <summary>
    /// Добавляет пользовательские claims из UserManager в соответствии с разрешенным списком claims
    /// </summary>
    /// <param name="identity">ClaimsIdentity для добавления claims</param>
    /// <param name="user">Пользователь, для которого добавляются claims</param>
    private async Task AddUserClaimsAsync(ClaimsIdentity identity, AppUser user)
    {
        // Получаем все пользовательские claims из UserManager
        var claims = await userManager.GetClaimsAsync(user);

        // Фильтруем claims через whitelist разрешенных claims и добавляем в identity
        identity.AddClaims(claims.Where(c => AllowedClaims.Contains(c.Type)));
    }

    /// <summary>
    /// Добавляет claims, связанные с электронной почтой пользователя, в соответствии со стандартами OpenID Connect
    /// </summary>
    /// <param name="identity">ClaimsIdentity для добавления claims</param>
    /// <param name="user">Пользователь, для которого добавляются claims</param>
    private async Task AddEmailClaimsAsync(ClaimsIdentity identity, AppUser user)
    {
        // Получаем email пользователя из UserManager
        var email = await userManager.GetEmailAsync(user);
        if (string.IsNullOrWhiteSpace(email)) return;

        // Проверяем подтвержден ли email
        var emailVerified = await userManager.IsEmailConfirmedAsync(user);

        // Добавляем OIDC claim 'email'
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Email, email));

        // Добавляем OIDC claim 'email_verified'
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
        // Получаем номер телефона пользователя из UserManager
        var phone = await userManager.GetPhoneNumberAsync(user);
        if (string.IsNullOrWhiteSpace(phone)) return;

        // Проверяем подтвержден ли номер телефона
        var phoneVerified = await userManager.IsPhoneNumberConfirmedAsync(user);

        // Добавляем OIDC claim 'phone_number'
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.PhoneNumber, phone));

        // Добавляем OIDC claim 'phone_number_verified'
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
        // Находим claims, которые нужно сохранить из текущей identity
        var preservedClaims = currentIdentity.Claims
            .Where(c => c.Type is OpenIddictConstants.Claims.AuthenticationMethodReference
                or Constants.Claims.IdentityProvider
                or OpenIddictConstants.Claims.AuthenticationTime || c.Type.StartsWith("oi_"));

        // Добавляем каждый сохраненный claim, если он отсутствует в целевой identity
        identity.AddClaims(preservedClaims);
    }

    /// <summary>
    /// Список разрешенных claims (утверждений) для использования в системе.
    /// Включает стандартные claims OpenIddict и дополнительные кастомные claims.
    /// </summary>
    private static readonly HashSet<string> AllowedClaims = typeof(OpenIddictConstants.Claims)

        // Получаем все публичные статические поля из класса OpenIddictConstants.Claims
        .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)

        // Извлекаем значения этих полей (строковые константы)
        .Select(f => (string)f.GetValue(null)!)

        // Добавляем кастомный claim Identity Provider
        .Concat([Constants.Claims.IdentityProvider])

        // Преобразовываем в хеш-сет
        .ToHashSet();

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
                if (claim.Subject!.HasScope(OpenIddictConstants.Scopes.Profile) && AllowedClaims.Contains(claim.Type))
                    yield return OpenIddictConstants.Destinations.IdentityToken;
                yield break;
        }
    }
}