namespace Common.Domain.Extensions;

/// <summary>
/// Методы расширения для валидации
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Проверяет длину строки
    /// </summary>
    /// <param name="value">Проверяемое значение</param>
    /// <param name="name">Название параметра для сообщения об ошибке</param>
    /// <param name="maxLength">Максимальная допустимая длина</param>
    /// <returns>Проверенное значение</returns>
    /// <exception cref="ArgumentException">Выбрасывается если значение не соответствует требованиям</exception>
    public static string ValidateLength(this string value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
            throw new ArgumentException($"Value {name} should not be null, empty or longer than {maxLength} characters.");

        return value;
    }
}