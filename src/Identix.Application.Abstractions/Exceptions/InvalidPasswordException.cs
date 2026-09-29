namespace Identix.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, возникающее при неверно введенном пароле.
/// </summary>
public class InvalidPasswordException() : Exception("Invalid password entered");