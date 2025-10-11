using Identix.Application.Abstractions.Enums;

namespace Identix.Application.Abstractions.Extensions;

/// <summary>
/// Класс вспомогательных методов для форматирования данных
/// </summary>
public static class LocalizationExtensions
{
    /// <summary>
    /// Русский язык
    /// </summary>
    public const string Ru = "ru";

    /// <summary>
    /// Английский язык
    /// </summary>
    public const string En = "en";

    /// <summary>
    /// Метод - расширение отдает локализацию из строки
    /// </summary>
    /// <param name="localization">Локализация</param>
    /// <returns>Локализация как строка</returns>
    public static Localization GetLocalization(this string? localization)
    {
        // проверяем входящие данные
        if (localization == null) return Localization.En;

        // смотрим локализацию в нижнем регистре и отдаем значение из enum
        return localization.ToLower() switch
        {
            Ru => Localization.Ru,
            _ => Localization.En
        };
    }
    
    /// <summary>
    /// Метод - расширение отдает локализацию из строки
    /// </summary>
    /// <param name="localization">Локализация</param>
    /// <returns>Локализация как строка</returns>
    public static string GetLocalizationString(this Localization localization)
    {
        // смотрим локализацию в нижнем регистре и отдаем значение из enum
        return localization.ToString().ToLower();
    }
}