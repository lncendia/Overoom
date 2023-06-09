using Overoom.Infrastructure.Storage.Models.Films;

namespace Overoom.Infrastructure.Storage.Models.Users;

public class WatchlistModel
{
    public long Id { get; set; }
    public Guid FilmId { get; set; }
    public FilmModel Film { get; set; } = null!;
}