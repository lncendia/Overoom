﻿namespace Watch2gether.Domain.Rooms.YoutubeRoom.Exceptions;

public class VideoInViewException : Exception
{
    public VideoInViewException() : base("A viewer is watching the video")
    {
    }
}