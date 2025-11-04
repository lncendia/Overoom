using Common.Application.FileStorage;
using MediatR;
using Identix.Application.Abstractions;
using Identix.Application.Abstractions.Queries;

namespace Identix.Application.Services.Queries;

/// <summary>
/// Обработчик запроса на получение фотографии пользователя из файлового хранилища
/// </summary>
/// <param name="fileStore">Сервис для работы с файловым хранилищем (например, S3)</param>
public class PhotoQueryHandler(IFileStorage fileStore) : IRequestHandler<PhotoQuery, FileResult>
{
    /// <summary>
    /// Обрабатывает запрос на получение фотографии пользователя
    /// </summary>
    /// <param name="request">Запрос, содержащий ключ фотографии пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат в виде файла (FileResult) с потоком данных фотографии</returns>
    public async Task<FileResult> Handle(PhotoQuery request, CancellationToken cancellationToken)
    {
        // Валидируем запрашиваемый ключ
        if (!IsValidUserPhotoKey(request.Key))
            throw new ArgumentException($"Invalid photo key format. Expected: {Constants.Storage.UserPhotoKeyFormat} or {Constants.Storage.ClientPhotoKeyFormat}");

        // Получаем объект из S3
        var (stream, contentType) = await fileStore.GetAsync(request.Key, token: cancellationToken);

        // Возвращаем файл
        return new FileResult(stream, contentType, request.Key);
    }

    /// <summary>
    /// Проверяет, соответствует ли ключ формату UserPhotoKeyFormat
    /// </summary>
    /// <param name="key">Ключ для проверки</param>
    /// <returns>True, если ключ соответствует формату</returns>
    private static bool IsValidUserPhotoKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;

        // Разбиваем путь на части
        var parts = key.Split('/');
        return parts.Length == 3;
    }
}