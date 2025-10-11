using AspNetCore.Identity.Mongo.Model;
using Identix.Application.Abstractions.Enums;
using MongoDB.Bson.Serialization.Attributes;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Extensions;

namespace Identix.Application.Abstractions.Entities;

/// <summary>
/// Класс, представляющий пользователя приложения.
/// Наследуется от <see cref="MongoUser{Guid}"/>, что обеспечивает базовую функциональность пользователя,
/// такую как хранение идентификатора, имени пользователя, пароля и ролей.
/// </summary>
public sealed class AppUser : MongoUser<Guid>
{
    /// <summary>
    /// Дата и время регистрации пользователя.
    /// Это обязательное свойство, которое указывает, когда пользователь зарегистрировался в системе.
    /// </summary>
    public required DateTime RegistrationTimeUtc { get; init; }

    /// <summary>
    /// Дата и время последней аутентификации пользователя.
    /// Это обязательное свойство, которое указывает, когда пользователь последний раз входил в систему.
    /// </summary>
    public required DateTime LastAuthTimeUtc { get; set; }

    /// <summary>
    /// Локализация пользователя, определяемая на основе его настроек.
    /// Это вычисляемое свойство, которое извлекает значение локализации из claims пользователя.
    /// Значение игнорируется при сериализации в MongoDB благодаря атрибуту <see cref="BsonIgnoreAttribute"/>.
    /// </summary>
    [BsonIgnore]
    public Localization Locale
    {
        get
        {
            // Ищем claim, соответствующий типу JwtClaimTypes.Locale.
            var claim = Claims.FirstOrDefault(c => c.ClaimType == OpenIddictConstants.Claims.Locale)?.ClaimValue;
            
            // Преобразуем значение claim в тип Localization.
            return claim.GetLocalization();
        }
    }

    /// <summary>
    /// URI аватара пользователя (миниатюра).
    /// Это вычисляемое свойство, которое извлекает URI аватара из claims пользователя.
    /// Значение игнорируется при сериализации в MongoDB благодаря атрибуту <see cref="BsonIgnoreAttribute"/>.
    /// Если claim отсутствует или его значение равно null, возвращается null.
    /// </summary>
    [BsonIgnore]
    public string? PhotoKey
    {
        get
        {
            // Ищем claim, соответствующий типу JwtClaimTypes.Picture.
            return Claims.FirstOrDefault(c => c.ClaimType ==  OpenIddictConstants.Claims.Picture)?.ClaimValue;
        }
    }
}