namespace Identix.Application.Abstractions.Exceptions;

/// <summary>
/// Исключения, возникающее при попытке смены пароля без указания старого, если старый пароль существует
/// </summary>
public class PasswordNeededException() : Exception("For this account, you must specify the password");