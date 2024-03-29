﻿using Overoom.Infrastructure.Storage.Models.Abstractions;
using Overoom.Infrastructure.Storage.Models.Film;
using Overoom.Infrastructure.Storage.Models.Genre;

namespace Overoom.Infrastructure.Storage.Models.User;

public class UserModel : IAggregateModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string EmailNormalized { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string NameNormalized { get; set; } = null!;
    public Uri AvatarUri { get; set; } = null!;
    public List<WatchlistModel> Watchlist { get; set; } = new();
    public List<HistoryModel> History { get; set; } = new();
    public List<GenreModel> Genres { get; set; } = new();
    
    
    public bool Beep { get; set; }
    public bool Scream { get; set; }
    public bool Change { get; set; }
}