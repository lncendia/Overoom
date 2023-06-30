﻿using Overoom.Domain.Abstractions;
using Overoom.Domain.Films.DTOs;
using Overoom.Domain.Films.Enums;
using Overoom.Domain.Films.Exceptions;
using Overoom.Domain.Films.ValueObject;

namespace Overoom.Domain.Films.Entities;

public class Film : AggregateRoot
{
    public Film(string name, string description, string? shortDescription, int year, double rating, FilmType type,
        IEnumerable<CdnDto> cdn, IEnumerable<string> genres, IEnumerable<(string name, string description)> actors,
        IEnumerable<string> directors, IEnumerable<string> screenwriters, IEnumerable<string> countries,
        Uri posterUri, int? countSeasons = null, int? countEpisodes = null)
    {
        FilmTags = new FilmTags(genres, countries, actors, directors, screenwriters);

        Type = type;
        Name = name;
        PosterUri = posterUri;
        Year = year;
        RatingKp = rating;
        Description = description;
        ShortDescription = shortDescription;
        Type = type;
        if (countSeasons != null && countEpisodes != null) UpdateSeriesInfo(countSeasons.Value, countEpisodes.Value);

        _cdnList = cdn.DistinctBy(x=>x.Type).Select(MapCdn).ToList();
        if (!_cdnList.Any()) throw new ArgumentException("Cdn list is empty");
    }

    public string Name { get; }
    public int Year { get; }
    public FilmType Type { get; }

    private readonly List<Cdn> _cdnList;
    public IReadOnlyCollection<Cdn> CdnList => _cdnList.AsReadOnly();
    public Uri PosterUri { get; set; }
    public FilmTags FilmTags { get; }

    private double _userRating;

    public double UserRating
    {
        get => _userRating;
        set
        {
            if (value is < 0 or > 10)
                throw new ArgumentException("Rating must be between 0 and 10");
            _userRating = value;
        }
    }

    private string _description = null!;

    public string Description
    {
        get => _description;
        set
        {
            if (value.Length > 1500) throw new DescriptionTooLongException();
            _description = value;
        }
    }

    private string? _shortDescription;

    public string? ShortDescription
    {
        get => _shortDescription;
        set
        {
            if (value?.Length > 500) throw new ShortDescriptionTooLongException();
            _shortDescription = value;
        }
    }

    public int? CountSeasons { get; private set; }
    public int? CountEpisodes { get; private set; }

    private double _rating;

    public double RatingKp
    {
        get => _rating;
        set
        {
            if (value is < 0 or > 10)
                throw new ArgumentException("Rating must be between 0 and 10");
            _rating = value;
        }
    }

    public void UpdateSeriesInfo(int countSeasons, int countEpisodes)
    {
        if (Type != FilmType.Serial)
            throw new InvalidOperationException("Count of episodes must be specified for serials");

        CountSeasons = countSeasons;
        CountEpisodes = countEpisodes;
    }

    public void AddOrChangeCdn(CdnDto cdn)
    {
        _cdnList.RemoveAll(x => x.Type == cdn.Type);
        _cdnList.Add(MapCdn(cdn));
    }

    private static Cdn MapCdn(CdnDto dto) => new(dto.Type, dto.Uri, dto.Quality, dto.Voices.ToList());
}