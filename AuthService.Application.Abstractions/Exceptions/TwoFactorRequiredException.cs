using AuthService.Application.Abstractions.Entities;

namespace AuthService.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, возникающее при прохождении аутентификации паролем со включенной 2FA
/// </summary>
/// <param name="user">Объект Identity пользователя</param>
public class TwoFactorRequiredException(UserData user) : Exception("Two factor authentication is required")
{
    /// <summary>
    /// Аутентифицированный пользователь
    /// </summary>
    public UserData User { get; } = user;
}