﻿using Films.Application.Abstractions.DTOs.Films;
using Films.Application.Abstractions.Queries.Films;
using Films.Infrastructure.Web.Authentication;
using Films.Infrastructure.Web.Common.ViewModels;
using Films.Infrastructure.Web.Films.InputModels;
using Films.Infrastructure.Web.Films.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Films.Infrastructure.Web.Films.Controllers;

[ApiController]
[Route("filmApi/[controller]/[action]")]
public class FilmsController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<FilmShortViewModel>> Popular([FromQuery] PopularFilmsInputModel model)
    {
        var popular = await mediator.Send(new PopularFilmsQuery
        {
            Take = model.Count
        });

        return popular.Select(Map);
    }

    [HttpGet]
    public async Task<ListViewModel<FilmShortViewModel>> Search([FromQuery] FilmsSearchInputModel model)
    {
        var data = await mediator.Send(new FindFilmsQuery
        {
            Country = model.Country,
            Query = model.Query,
            Genre = model.Genre,
            Person = model.Person,
            Serial = model.Serial,
            PlaylistId = model.PlaylistId,
            MinYear = model.MinYear,
            MaxYear = model.MaxYear,
            Skip = (model.Page - 1) * model.CountPerPage,
            Take = model.CountPerPage
        });

        var countPages = data.TotalCount / model.CountPerPage;

        if (data.TotalCount % model.CountPerPage > 0) countPages++;

        return new ListViewModel<FilmShortViewModel>
        {
            TotalPages = countPages,
            List = data.List.Select(Map)
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<FilmViewModel> Get(Guid id)
    {
        Guid? userId = User.Identity is { IsAuthenticated: true } ? User.GetId() : null;

        var film = await mediator.Send(new FilmByIdQuery
        {
            Id = id,
            UserId = userId
        });

        return Map(film);
    }


    [HttpGet]
    [Authorize]
    public async Task<IEnumerable<FilmShortViewModel>> Watchlist()
    {
        var films = await mediator.Send(new UserWatchlistQuery
        {
            Id = User.GetId(),
        });

        return films.Select(Map);
    }

    [HttpGet]
    [Authorize]
    public async Task<IEnumerable<FilmShortViewModel>> History()
    {
        var films = await mediator.Send(new UserHistoryQuery
        {
            Id = User.GetId()
        });

        return films.Select(Map);
    }

    private FilmViewModel Map(FilmDto film) => new()
    {
        Id = film.Id,
        Description = film.Description,
        IsSerial = film.IsSerial,
        Year = film.Year,
        Title = film.Title,
        PosterUrl = $"{Request.Scheme}://{Request.Host}/{film.PosterUrl.ToString().Replace('\\', '/')}",
        RatingKp = film.RatingKp,
        RatingImdb = film.RatingImdb,
        UserRating = film.UserRating,
        UserRatingsCount = film.UserRatingsCount,
        UserScore = film.UserScore,
        InWatchlist = film.InWatchlist,
        CdnList = film.CdnList.Select(cdn => new CdnViewModel
        {
            Cdn = cdn.Name,
            Quality = cdn.Quality,
            Url = cdn.Url.ToString()
        }),
        CountSeasons = film.CountSeasons,
        CountEpisodes = film.CountEpisodes,
        Genres = film.Genres,
        Countries = film.Countries,
        Directors = film.Directors,
        ScreenWriters = film.ScreenWriters,
        Actors = film.Actors.Select(a => new ActorViewModel
        {
            Description = a.Description,
            Name = a.Name
        })
    };

    private FilmShortViewModel Map(FilmShortDto film) => new()
    {
        Id = film.Id,
        Title = film.Title,
        PosterUrl = $"{Request.Scheme}://{Request.Host}/{film.PosterUrl.ToString().Replace('\\', '/')}",
        UserRating = film.UserRating,
        RatingKp = film.RatingKp,
        RatingImdb = film.RatingImdb,
        Description = film.Description,
        IsSerial = film.IsSerial,
        Genres = film.Genres,
        Year = film.Year,
        CountSeasons = film.CountSeasons,
        CountEpisodes = film.CountEpisodes
    };
}