﻿using System.Reflection;
using Overoom.Domain.Abstractions;
using Overoom.Domain.Films.DTOs;
using Overoom.Domain.Films.Entities;
using Overoom.Domain.Films.Enums;
using Overoom.Infrastructure.Storage.Mappers.Abstractions;
using Overoom.Infrastructure.Storage.Mappers.StaticMethods;
using Overoom.Infrastructure.Storage.Models.Film;

namespace Overoom.Infrastructure.Storage.Mappers.AggregateMappers;

internal class FilmMapper : IAggregateMapperUnit<Film, FilmModel>
{
    private static readonly FieldInfo UserRating =
        typeof(Film).GetField("<UserRating>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly FieldInfo UserRatingsCount =
        typeof(Film).GetField("<UserRatingsCount>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;

    public Film Map(FilmModel model)
    {
        var builder = FilmBuilder.Create()
            .WithActors(model.Actors.Select(x => (x.Person.Name, x.Description)))
            .WithCdn(model.CdnList.Select(x =>
                new CdnDto(x.Type, x.Uri, x.Quality, x.Voices.Select(voiceModel => voiceModel.Name).ToList())))
            .WithCountries(model.Countries.Select(x => x.Name))
            .WithDescription(model.Description)
            .WithYear(model.Year)
            .WithDirectors(model.Directors.Select(x => x.Name))
            .WithGenres(model.Genres.Select(x => x.Name))
            .WithName(model.Name)
            .WithPoster(model.PosterUri)
            .WithRating(model.Rating)
            .WithScreenwriters(model.ScreenWriters.Select(x => x.Name))
            .WithType(model.Type);

        if (!string.IsNullOrEmpty(model.ShortDescription))
            builder = builder.WithShortDescription(model.ShortDescription);
        if (model.Type == FilmType.Serial)
            builder = builder.WithEpisodes(model.CountSeasons!.Value, model.CountEpisodes!.Value);
        var film = builder.Build();
        IdFields.AggregateId.SetValue(film, model.Id);
        var domainCollection = (List<IDomainEvent>)IdFields.DomainEvents.GetValue(film)!;
        domainCollection.Clear();
        UserRating.SetValue(film, model.UserRating);
        UserRatingsCount.SetValue(film, model.UserRatingsCount);
        return film;
    }
}