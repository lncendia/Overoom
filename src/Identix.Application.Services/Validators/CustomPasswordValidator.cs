using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Entities;

namespace Identix.Application.Services.Validators;

/// <summary>
/// Класс валидатора для пароля
/// </summary>
public partial class CustomPasswordValidator : IPasswordValidator<AppUser>
{
  /// <summary>
  /// Метод валидации пароля
  /// </summary>
  /// <param name="manager">Менеджер пользователей (UserManager)</param>
  /// <param name="user">Пользователь, для которого проводится валидация</param>
  /// <param name="password">Пароль, который требуется проверить</param>
  /// <returns>Возвращает результат валидации в виде объекта IdentityResult</returns>
  public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string? password)
  {
    password = PasswordRegex().Replace(password!, " ");

    if (password.Length is < 8 or > 128)
    {
      return Task.FromResult(IdentityResult.Failed(new IdentityError
      {
        Description = "Password length should be from 8 to 128 characters.",
        Code = "PasswordLengthInvalid"
      }));
    }

    bool hasUpperChar = password.Any(char.IsUpper);
    bool hasLowerChar = password.Any(char.IsLower);
    bool hasDigit = password.Any(char.IsDigit);
    bool hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));

    if (hasUpperChar && hasLowerChar && hasDigit && hasSpecialChar)
    {
      return Task.FromResult(IdentityResult.Success);
    }

    var errors = new List<IdentityError>();

    if (!hasUpperChar)
    {
      errors.Add(new IdentityError
        { Description = "Password must contain uppercase letters.", Code = "PasswordRequiresUpper" });
    }

    if (!hasLowerChar)
    {
      errors.Add(new IdentityError
        { Description = "Password must contain lowercase letters.", Code = "PasswordRequiresLower" });
    }

    if (!hasDigit)
    {
      errors.Add(new IdentityError { Description = "Password must contain digits.", Code = "PasswordRequiresDigit" });
    }

    if (!hasSpecialChar)
    {
      errors.Add(new IdentityError
        { Description = "Password must contain special characters.", Code = "PasswordRequiresNonAlphanumeric" });
    }

    return Task.FromResult(IdentityResult.Failed([.. errors]));
  }

  [GeneratedRegex(@"\s+")]
  private static partial Regex PasswordRegex();
}