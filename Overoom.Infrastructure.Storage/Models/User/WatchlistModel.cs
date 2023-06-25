using Overoom.Infrastructure.Storage.Models.Film;

namespace Overoom.Infrastructure.Storage.Models.User;

public class WatchlistModel
{
    public long Id { get; set; }
    public Guid FilmId { get; set; }
    public FilmModel Film { get; set; } = null!;
}