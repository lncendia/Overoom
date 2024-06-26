﻿using System.Reflection;
using Films.Domain.Abstractions;
using Films.Domain.Films;
using Films.Domain.Films.ValueObjects;
using Films.Infrastructure.Storage.Mappers.Abstractions;
using Films.Infrastructure.Storage.Mappers.StaticMethods;
using Films.Infrastructure.Storage.Models.Films;

namespace Films.Infrastructure.Storage.Mappers.AggregateMappers;

internal class FilmMapper : IAggregateMapperUnit<Film, FilmModel>
{
    private static readonly FieldInfo UserRating =
        typeof(Film).GetField("<UserRating>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly FieldInfo UserRatingsCount =
        typeof(Film).GetField("<UserRatingsCount>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;

    public Film Map(FilmModel model)
    {
        var film = new Film(model.IsSerial, model.CountSeasons, model.CountEpisodes)
        {
            Title = model.Title,
            Description = model.Description,
            Year = model.Year,
            PosterUrl = model.PosterUrl,
            RatingKp = model.RatingKp,
            RatingImdb = model.RatingImdb,
            Genres = model.Genres.Select(x => x.Name).ToArray(),
            Countries = model.Countries.Select(x => x.Name).ToArray(),
            Directors = model.Directors.Select(x => x.Name).ToArray(),
            Actors = model.Actors.Select(x => new Actor(x.Person.Name, x.Description)).ToArray(),
            Screenwriters = model.Screenwriters.Select(x => x.Name).ToArray(),
            CdnList = model.CdnList.Select(cdn => new Cdn
            {
                Name = cdn.Name,
                Url = cdn.Url,
                Quality = cdn.Quality
            }).ToArray()
        };

        if (!string.IsNullOrEmpty(model.ShortDescription)) film.ShortDescription = model.ShortDescription;

        UserRating.SetValue(film, model.UserRating);
        UserRatingsCount.SetValue(film, model.UserRatingsCount);

        IdFields.AggregateId.SetValue(film, model.Id);
        var domainCollection = (List<IDomainEvent>)IdFields.DomainEvents.GetValue(film)!;
        domainCollection.Clear();
        return film;
    }
}