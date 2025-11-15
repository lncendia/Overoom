namespace Common.DI.Middlewares;

using System.Security.Cryptography;

/// <summary>
/// Утилита для безопасного хэширования и верификации паролей.
/// Использует алгоритм PBKDF2 с SHA-256 для защиты от brute-force атак.
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// Размер соли в байтах (128 бит).
    /// Соль добавляет случайность к каждому хэшу, предотвращая атаки по радужным таблицам.
    /// </summary>
    private const int SaltSize = 16; // 128 бит
    
    /// <summary>
    /// Размер ключа (хэша) в байтах (256 бит).
    /// Определяет длину итогового хэшированного значения.
    /// </summary>
    private const int KeySize = 32;  // 256 бит
    
    /// <summary>
    /// Количество итераций алгоритма PBKDF2 (100,000).
    /// Увеличивает стоимость подбора пароля, защищая от brute-force атак.
    /// </summary>
    private const int Iterations = 100_000;
    
    /// <summary>
    /// Используемый алгоритм хэширования - SHA-256.
    /// </summary>
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Создает безопасный хэш пароля.
    /// Генерирует случайную соль и применяет алгоритм PBKDF2 для создания хэша.
    /// </summary>
    /// <param name="password">Пароль в открытом виде для хэширования</param>
    /// <returns>Строка в формате "соль.хэш" где обе части в Base64</returns>
    public static string Hash(string password)
    {
        // Генерируем криптографически безопасную случайную соль
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        
        // Создаем ключ (хэш) используя PBKDF2 с солью и множеством итераций
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        
        // Возвращаем соль и хэш в формате Base64, разделенные точкой
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    /// <summary>
    /// Проверяет соответствие пароля хранимому хэшу.
    /// Использует сравнение с постоянным временем выполнения для защиты от timing-атак.
    /// </summary>
    /// <param name="password">Пароль в открытом виде для проверки</param>
    /// <param name="hash">Хранимый хэш в формате "соль.хэш"</param>
    /// <returns>true - если пароль соответствует хэшу, false - в противном случае</returns>
    public static bool Verify(string password, string hash)
    {
        // Разделяем хэш на соль и ключ
        var parts = hash.Split('.');
        if (parts.Length != 2) return false;

        // Декодируем соль и ключ из Base64
        var salt = Convert.FromBase64String(parts[0]);
        var key = Convert.FromBase64String(parts[1]);
        
        // Вычисляем хэш для предоставленного пароля с той же солью
        var attemptedKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        
        // Сравниваем хэши с постоянным временем выполнения
        return CryptographicOperations.FixedTimeEquals(attemptedKey, key);
    }
}