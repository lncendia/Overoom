namespace Uploader.Application.Abstractions.Jobs;

/// <summary>
/// Этап обработки фильма, на котором находится задача. Этапы выполняются по порядку,
/// при повторном запуске задача продолжает с сохранённого этапа.
/// </summary>
public enum JobStage
{
  /// <summary>
  /// Скачивание торрента
  /// </summary>
  Downloading,

  /// <summary>
  /// Транскодирование в HLS
  /// </summary>
  Transcoding,

  /// <summary>
  /// Загрузка в файловое хранилище
  /// </summary>
  Uploading,

  /// <summary>
  /// Публикация события о готовности фильма
  /// </summary>
  Publishing
}
