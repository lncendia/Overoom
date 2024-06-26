using Films.Infrastructure.Storage.Models.Films;
using Films.Infrastructure.Storage.Models.Playlists;
using Films.Infrastructure.Storage.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Films.Infrastructure.Storage.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<FilmModel> LoadDependencies(this IQueryable<FilmModel> queryable)
    {
        return queryable
            .Include(f => f.Actors)
            .ThenInclude(a => a.Person)
            .Include(f => f.Countries)
            .Include(f => f.Directors)
            .Include(f => f.Genres)
            .Include(f => f.Screenwriters)
            .Include(f => f.CdnList);
    }

    public static IQueryable<PlaylistModel> LoadDependencies(this IQueryable<PlaylistModel> queryable)
    {
        return queryable
            .Include(p => p.Films)
            .Include(p => p.Genres);
    }
    
    public static IQueryable<UserModel> LoadDependencies(this IQueryable<UserModel> queryable)
    {
        return queryable
            .Include(u => u.Watchlist)
            .Include(u => u.History)
            .Include(u => u.Genres);
    }
}