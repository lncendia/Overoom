﻿namespace Overoom.WEB.Models.Rooms.FilmRoom;

public class FilmViewerViewModel : ViewerViewModel
{
    public int Season { get; }
    public int Series { get; }

    public FilmViewerViewModel(int id, string username, Uri avatarUri, bool pause, TimeSpan time, int season,
        int series, bool fullScreen, bool allowBeep, bool allowScream, bool allowChange) : base(id, username, avatarUri,
        pause, time, fullScreen, allowBeep, allowScream, allowChange)
    {
        Season = season;
        Series = series;
    }
}