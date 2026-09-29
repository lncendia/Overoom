using System.Security.Claims;
using Identix.Application.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace Identix.Infrastructure.Web.Account.Services;

/// <inheritdoc/>
public class OpenIdSignInManager<TUser>(
  UserManager<TUser> userManager,
  IHttpContextAccessor contextAccessor,
  IUserClaimsPrincipalFactory<TUser> claimsFactory,
  IOptions<IdentityOptions> optionsAccessor,
  ILogger<SignInManager<TUser>> logger,
  IAuthenticationSchemeProvider schemes,
  IUserConfirmation<TUser> confirmation)
  : SignInManager<TUser>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
  where TUser : class
{
  /// <inheritdoc/>
  public override async Task SignInWithClaimsAsync(TUser user, AuthenticationProperties? authenticationProperties,
    IEnumerable<Claim> additionalClaims)
  {
    ClaimsPrincipal userPrincipal = await CreateUserPrincipalAsync(user);

    foreach (Claim claim in additionalClaims)
    {
      userPrincipal.Identities.First().AddClaim(claim);
    }

    AugmentMissingClaims(userPrincipal);

    await Context.SignInAsync(
      AuthenticationScheme,
      userPrincipal,
      authenticationProperties ?? new AuthenticationProperties());

    Context.User = userPrincipal;
  }

  /// <summary>
  /// Дополняет объект ClaimsPrincipal недостающими claims (утверждениями) для корректной работы с OpenIddict.
  /// Выполняет преобразование и добавление стандартных утверждений, необходимых для аутентификации и авторизации.
  /// </summary>
  /// <param name="principal">Объект ClaimsPrincipal, который требуется дополнить утверждениями</param>
  private static void AugmentMissingClaims(ClaimsPrincipal principal)
  {
    ClaimsIdentity identity = principal.Identities.First();

    // ASP.NET Identity использует этот тип claim с именем провайдера аутентификации (например, "Google")
    // Этот код преобразует его в стандартные claims для нашего сценария
    Claim? amr = identity.FindFirst(ClaimTypes.AuthenticationMethod);

    if (amr != null && identity.FindFirst(Constants.Claims.IdentityProvider) == null &&
        identity.FindFirst(OpenIddictConstants.Claims.AuthenticationMethodReference) == null)
    {
      identity.RemoveClaim(amr);
      identity.AddClaim(new Claim(Constants.Claims.IdentityProvider, amr.Value));

      identity.AddClaim(new Claim(OpenIddictConstants.Claims.AuthenticationMethodReference,
        Constants.AuthenticationMethods.External));
    }

    if (identity.FindFirst(Constants.Claims.IdentityProvider) == null)
    {
      identity.AddClaim(new Claim(Constants.Claims.IdentityProvider, Constants.IdentityProviders.Local));
    }

    if (identity.FindFirst(OpenIddictConstants.Claims.AuthenticationMethodReference) == null)
    {
      if (identity.FindFirst(Constants.Claims.IdentityProvider)?.Value == Constants.IdentityProviders.Local)
      {
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.AuthenticationMethodReference,
          Constants.AuthenticationMethods.Password));
      }
      else
      {
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.AuthenticationMethodReference,
          Constants.AuthenticationMethods.External));
      }
    }

    // Claim auth_time требуется стандартом OpenID Connect
    if (identity.FindFirst(OpenIddictConstants.Claims.AuthenticationTime) == null)
    {
      string time = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds().ToString();

      identity.AddClaim(new Claim(OpenIddictConstants.Claims.AuthenticationTime, time,
        ClaimValueTypes.Integer64));
    }
  }
}