using Overoom.Application.Abstractions.FilmsManagement.DTOs;
using Overoom.Application.Abstractions.FilmsManagement.Interfaces;
using Overoom.Domain.Films.Entities;

namespace Overoom.Application.Services.FilmsManagement;

public class FilmManagementMapper : IFilmManagementMapper
{
    public FilmDto MapGet(Film film)
    {
        var cdnList = film.CdnList.Select(x => new CdnDto(x.Type, x.Uri, x.Quality, x.Voices)).ToList();
        return new FilmDto(film.Id, film.Description, film.ShortDescription, film.Rating, cdnList,
            film.CountSeasons, film.CountEpisodes, film.Name);
    }

    public FilmShortDto MapShort(Film film) => new(film.Name, film.Year, film.Type, film.PosterUri, film.Id);
}