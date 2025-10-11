namespace Films.Infrastructure.Web.Components.Interfaces;

/// <summary>
/// Интерфейс для моделей, содержащий информацию об запрашиваемой порции
/// </summary>
public interface IWithInputPagination
{
    /// <summary>
    /// Лимит.
    /// </summary>
    public int Take { get; init; }

    /// <summary>
    /// Смещение.
    /// </summary>
    public int Skip { get; init; }
}