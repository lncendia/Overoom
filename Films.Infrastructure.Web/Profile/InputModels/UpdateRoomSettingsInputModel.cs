namespace Films.Infrastructure.Web.Profile.InputModels;

/// <summary>
/// Модель данных для изменения разрешений/настроек
/// </summary>
public class UpdateRoomSettingsInputModel
{
    /// <summary>
    /// Разрешение на звуковой сигнал
    /// </summary>
    public bool Beep { get; init; }

    /// <summary>
    /// Разрешение на скример
    /// </summary>
    public bool Screamer { get; init; }
}