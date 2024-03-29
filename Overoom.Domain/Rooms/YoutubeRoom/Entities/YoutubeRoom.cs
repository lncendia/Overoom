﻿using Overoom.Domain.Rooms.BaseRoom.DTOs;
using Overoom.Domain.Rooms.BaseRoom.Entities;
using Overoom.Domain.Rooms.BaseRoom.Exceptions;
using Overoom.Domain.Rooms.YoutubeRoom.Exceptions;

namespace Overoom.Domain.Rooms.YoutubeRoom.Entities;

public class YoutubeRoom : Room
{
    public YoutubeRoom(Uri url, bool access, bool isOpen, ViewerDto viewer) : base(isOpen,
        CreateViewer(viewer, GetId(url)))
    {
        Access = access;
        _ids.Add(GetId(url));
    }

    private static Viewer CreateViewer(ViewerDto viewer, string id) => new YoutubeViewer(viewer, 1, id);


    public bool Access { get; }
    public new YoutubeViewer Owner => (YoutubeViewer)base.Owner;
    public new IReadOnlyCollection<YoutubeViewer> Viewers => base.Viewers.Cast<YoutubeViewer>().ToList();
    public IReadOnlyCollection<string> VideoIds => _ids.ToList();
    private readonly List<string> _ids = new();

    public int Connect(ViewerDto viewer)
    {
        var youtubeViewer = new YoutubeViewer(viewer, GetNextId(), Owner.CurrentVideoId);
        return AddViewer(youtubeViewer);
    }

    public void ChangeVideo(int viewerId, string id)
    {
        if (!VideoIds.Contains(id)) throw new VideoNotFoundException();
        var viewer = (YoutubeViewer)GetViewer(viewerId);
        viewer.CurrentVideoId = id;
    }

    public string AddVideo(int viewerId, Uri url)
    {
        if (viewerId != Owner.Id && !Access) throw new ActionNotAllowedException();
        var id = GetId(url);
        _ids.Add(id);
        return id;
    }
    
    private static string GetId(Uri uri)
    {
        string id;
        try
        {
            id = uri.Host switch
            {
                "www.youtube.com" => uri.Query[3..],
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