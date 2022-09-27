﻿namespace Watch2gether.Application.Abstractions.DTO.Rooms;

public class RoomDto
{
    public RoomDto(List<MessageDto> messages, List<ViewerDto> viewers, Guid ownerId)
    {
        Messages = messages;
        Viewers = viewers;
        OwnerId = ownerId;
    }

    public Guid OwnerId { get; }
    public List<MessageDto> Messages { get; }
    public List<ViewerDto> Viewers { get; }
}

public class MessageDto
{
    public MessageDto(string text, DateTime createdAt, ViewerDto viewer)
    {
        Text = text;
        CreatedAt = createdAt;
        Viewer = viewer;
    }

    public ViewerDto Viewer { get; }
    public DateTime CreatedAt { get; }
    public string Text { get; }
}

public class ViewerDto
{
    public ViewerDto(string username, Guid id, string avatarUrl, TimeSpan time, bool onPause)
    {
        Username = username;
        Id = id;
        AvatarUrl = avatarUrl;
        Time = time;
        OnPause = onPause;
    }

    public Guid Id { get; }
    public string Username { get; }
    public string AvatarUrl { get; }
    public bool OnPause { get; }
    public TimeSpan Time { get; }
}