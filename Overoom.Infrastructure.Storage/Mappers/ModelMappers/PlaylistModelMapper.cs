﻿using Microsoft.EntityFrameworkCore;
using Overoom.Domain.Playlists.Entities;
using Overoom.Infrastructure.Storage.Context;
using Overoom.Infrastructure.Storage.Mappers.Abstractions;
using Overoom.Infrastructure.Storage.Models.Playlist;

namespace Overoom.Infrastructure.Storage.Mappers.ModelMappers;

internal class PlaylistModelMapper : IModelMapperUnit<PlaylistModel, Playlist>
{
    private readonly ApplicationDbContext _context;

    public PlaylistModelMapper(ApplicationDbContext context) => _context = context;

    public async Task<PlaylistModel> MapAsync(Playlist entity)
    {
        var playlist = await _context.Playlists.FirstOrDefaultAsync(x => x.Id == entity.Id);
        if (playlist != null)
        {
            await _context.Entry(playlist).Collection(x => x.Films).LoadAsync();
            await _context.Entry(playlist).Collection(x => x.Genres).LoadAsync();
        }
        else
            playlist = new PlaylistModel
            {
                Id = entity.Id,
                Genres = entity.Genres.Select(x => new PlaylistGenreModel { Name = x, NameNormalized = x.ToUpper() })
                    .ToList()
            };

        playlist.Name = entity.Name;
        playlist.Description = entity.Description;
        playlist.Updated = entity.Updated;


        var newFilms = entity.Films.Where(x => playlist.Films.All(m => m.FilmId != x));
        playlist.Films.AddRange(newFilms.Select(x => new PlaylistFilmModel { FilmId = x }));
        _context.PlaylistFilms.RemoveRange(playlist.Films.Where(x => entity.Films.All(m => m != x.FilmId)));


        return playlist;
    }
}