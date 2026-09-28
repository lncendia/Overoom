using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Entities;

namespace Identix.Application.Services.Validators;

/// <summary>
/// Класс валидатора для пользователя
/// </summary>
public class CustomUserValidator : IUserValidator<AppUser>
{
  /// <summary>
  /// Проверяет пользователя на наличие ошибок валидации.
  /// </summary>
  /// <param name="userManager">Менеджер пользователей.</param>
  /// <param name="user">Пользователь для валидации.</param>
  /// <returns>Результат валидации.</returns>
  public async Task<IdentityResult> ValidateAsync(UserManager<AppUser> userManager, AppUser user)
  {
    var errors = new List<IdentityError>();
    ValidateUserName(user, errors);
    await ValidateEmailAsync(user, userManager, errors);

    return errors.Count > 0 ? IdentityResult.Failed([.. errors]) : IdentityResult.Success;
  }

  /// <summary>
  /// Проверяет имя пользователя на валидность.
  /// </summary>
  /// <param name="user">Пользователь для проверки.</param>
  /// <param name="errors">Коллекция ошибок.</param>
  private static void ValidateUserName(AppUser user, ICollection<IdentityError> errors)
  {
    if (string.IsNullOrWhiteSpace(user.UserName) || user.UserName.Length > 40)
    {
      errors.Add(new IdentityError
      {
        Description = "The username must be up to 40 characters long.",

        Code = "InvalidUserNameLength"
      });
    }
  }

  /// <summary>
  /// Проверяет электронную почту пользователя на наличие ошибок валидации.
  /// </summary>
  /// <param name="user">Пользователь для валидации.</param>
  /// <param name="manager">Менеджер пользователей.</param>
  /// <param name="errors">Коллекция ошибок валидации.</param>
  private static async Task ValidateEmailAsync(AppUser user, UserManager<AppUser> manager,
    ICollection<IdentityError> errors)
  {
    string? email = user.Email;

    if (string.IsNullOrWhiteSpace(email))
    {
      errors.Add(new IdentityError
      {
        Description = "The mail cannot be empty.",

        Code = "InvalidEmail"
      });
      return;
    }

    try
    {
      _ = new MailAddress(email);
    }
    catch (FormatException)
    {
      errors.Add(new IdentityError
      {
        Description = "Invalid mail format.",

        Code = "InvalidEmail"
      });
      return;
    }

    AppUser? owner = await manager.FindByEmailAsync(email);

    if (owner != null && !owner.Id.Equals(user.Id))
    {
      errors.Add(new IdentityError
      {
        Description = $"The mail {user.Email} is already in use.",

        Code = "DuplicateEmail"
      });
    }
  }
}