﻿using Overoom.Domain.Films.Enums;

namespace Overoom.Application.Abstractions.FilmsManagement.DTOs;

public class LoadDto
{
    public LoadDto(string name, string description, string? shortDescription, double rating, int year,
        FilmType type, Uri? posterUri, Stream? posterStream, IReadOnlyCollection<string> genres,
        IReadOnlyCollection<(string name, string? description)> actors,
        IReadOnlyCollection<string> countries, IReadOnlyCollection<string> directors,
        IReadOnlyCollection<string> screenwriters, IReadOnlyCollection<CdnDto> cdnList,
        int? countSeasons, int? countEpisodes)
    {
        Name = name;
        Year = year;
        Type = type;
        PosterUri = posterUri;
        PosterStream = posterStream;
        CountSeasons = countSeasons;
        CountEpisodes = countEpisodes;
        CdnList = cdnList;
        Description = description;
        Rating = rating;
        ShortDescription = shortDescription;
        Genres = genres;
        Actors = actors;
        Countries = countries;
        Screenwriters = screenwriters;
        Directors = directors;
    }

    public string Description { get; }
    public string? ShortDescription { get; }
    public FilmType Type { get; }
    public Uri? PosterUri { get; }
    public Stream? PosterStream { get; }
    public string Name { get; }
    public int Year { get; }
    public double Rating { get; }
    public int? CountSeasons { get; }
    public int? CountEpisodes { get; }

    public IReadOnlyCollection<CdnDto> CdnList { get; }
    public IReadOnlyCollection<string> Countries { get; }
    public IReadOnlyCollection<(string name, string? description)> Actors { get; }
    public IReadOnlyCollection<string> Directors { get; }
    public IReadOnlyCollection<string> Genres { get; }
    public IReadOnlyCollection<string> Screenwriters { get; }
}