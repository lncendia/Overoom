namespace Identix.Application.Abstractions;

/// <summary>
/// Класс с константами для приложения Identix
/// </summary>
public static class Constants
{
    /// <summary>
    /// Константы для настройки Hangfire - системы фоновых заданий
    /// </summary>
    public static class Hangfire
    {
        /// <summary>
        /// Название очереди для обработки заданий загрузки
        /// </summary>
        public const string Queue = "identix";
    }
    
    /// <summary>
    /// Константы для работы с файлами и хранилищем
    /// </summary>
    public static class Storage
    {
        /// <summary>
        /// Формат ключа для хранения фото пользователя
        /// {0} заменяется на ID пользователя
        /// </summary>
        public const string UserPhotoKeyFormat = "user/thumbnail/{0}";
        
        /// <summary>
        /// MIME-тип для JPEG фото
        /// </summary>
        public const string JpegMimeType = "image/jpeg";
    }
    
    /// <summary>
    /// Константы для claims (утверждений) аутентификации
    /// </summary>
    public static class Claims
    {
        /// <summary>
        /// Claim для хранения ссылки на фото пользователя
        /// </summary>
        public const string Thumbnail = "urn:claims:picture";
        
        /// <summary>
        /// Claim типа идентификатора провайдера аутентификации
        /// </summary>
        public const string IdentityProvider = "idp";
    }
    
    /// <summary>
    /// Константы для провайдеров аутентификации
    /// </summary>
    public static class IdentityProviders
    {
        /// <summary>
        /// Локальный провайдер аутентификации
        /// </summary>
        public const string Local = "local";
    }
    
    /// <summary>
    /// Константы для методов аутентификации
    /// </summary>
    public static class AuthenticationMethods
    {
        /// <summary>
        /// Аутентификация по паролю
        /// </summary>
        public const string Password = "pwd";
        
        /// <summary>
        /// Внешняя аутентификация
        /// </summary>
        public const string External = "external";
    }
}