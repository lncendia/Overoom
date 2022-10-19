﻿using Watch2gether.Application.Abstractions.Exceptions.Rooms;
using Watch2gether.Domain.Rooms.BaseRoom.Exceptions;
using Watch2gether.Domain.Rooms.BaseRoom.ValueObject;
using Watch2gether.Domain.Rooms.YoutubeRoom.Entities;
using Watch2gether.Domain.Rooms.YoutubeRoom.Exceptions;

namespace Watch2gether.Domain.Rooms.YoutubeRoom;

public class YoutubeRoom : BaseRoom.BaseRoom
{
    public YoutubeRoom(string url, string name, string avatarFileName, bool addAccess)
    {
        AddAccess = addAccess;
        var id = GetId(url);
        VideoIds.Add(id);
        Owner = new YoutubeViewer(name, Id, avatarFileName, id);
        ViewersList.Add(Owner);
    }

    public bool AddAccess { get; }
    public List<Message> Messages => MessagesList.ToList();
    public YoutubeViewer Owner { get; }
    public List<YoutubeViewer> Viewers => ViewersList.Cast<YoutubeViewer>().ToList();
    public List<string> VideoIds => _ids.ToList();
    private readonly List<string> _ids = new();

    public YoutubeViewer Connect(string name, string avatarFileName)
    {
        if (ViewersList.Count >= 10) throw new RoomIsFullException();

        var viewer = new YoutubeViewer(name, Id, avatarFileName, Owner.CurrentVideoId);
        ViewersList.Add(viewer);
        return viewer;
    }

    public void ChangeVideo(Guid viewerId, string id)
    {
        if (!VideoIds.Contains(id)) throw new VideoNotFoundException();
        var viewer = (YoutubeViewer) GetViewer(viewerId);
        viewer.CurrentVideoId = id;
    }

    public string AddVideo(Guid viewerId, string url)
    {
        if (viewerId != Owner.Id && !AddAccess) throw new AddVideoException();
        var id = GetId(url);
        _ids.Add(id);
        return id;
    }

    public void RemoveId(string id)
    {
        if (_ids.Count == 1) throw new LastVideoException();
        if (ViewersList.Any(x => ((YoutubeViewer) x).CurrentVideoId == id))
            throw new VideoInViewException();
        _ids.Remove(id);
    }

    private static string GetId(string url)
    {
        var uri = new Uri(url);
        string id;
        try
        {
            id = uri.Host switch
            {
                "www.youtube.com" => uri.Query[..3],
                "youtu.be" => uri.Segments[1],
                _ => string.Empty
            };
        }
        catch
        {
            throw new InvalidVideoUrlException();
        }

        if (string.IsNullOrEmpty(id)) throw new InvalidVideoUrlException();
        return id;
    }
}