using Common.Application.FileStorage;
using Films.Application.Abstractions;
using Films.Application.Abstractions.Queries.Media;
using MediatR;

namespace Films.Application.Services.QueryHandlers.Media;

/// <summary>
/// Обработчик запроса на получение фотографии
/// </summary>
/// <param name="fileStore">Сервис для работы с файловым хранилищем</param>
public class GetPhotoQueryHandler(IFileStorage fileStore) : IRequestHandler<GetPhotoQuery, FileResult>
{
    /// <summary>
    /// Обрабатывает запрос на получение фотографии из файлового хранилища
    /// </summary>
    /// <param name="request">Запрос на получение фотографии</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат с потоком файла фотографии и информацией о content type</returns>
    /// <exception cref="ArgumentException">Выбрасывается при некорректном формате ключа фотографии</exception>
    /// <exception cref="FileNotFoundException">Выбрасывается если файл фотографии не найден в хранилище</exception>
    public async Task<FileResult> Handle(GetPhotoQuery request, CancellationToken cancellationToken)
    {
        // Валидируем запрашиваемый ключ
        if (!IsValidUserPhotoKey(request.Key))
            throw new ArgumentException($"Invalid user photo key format. Expected: {Constants.Poster.FilmKeyFormat} or {Constants.Poster.PlaylistKeyFormat}");

        // Получаем объект из S3
        var (stream, contentType) =  await fileStore.GetAsync(request.Key, token: cancellationToken);

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
        if (string.IsNullOrWhiteSpace(key))
            return false;

        // Разбиваем путь на части
        var parts = key.Split('/');
        if (parts.Length != 3)
            return false;

        // Проверяем префикс пути
        return (parts[0] == "film" || parts[0] == "playlist") && parts[1] == "poster";
    }
}