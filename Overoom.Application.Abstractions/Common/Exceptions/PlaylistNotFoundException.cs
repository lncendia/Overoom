﻿namespace Overoom.Application.Abstractions.Common.Exceptions;

public class PlaylistNotFoundException : Exception
{
    public PlaylistNotFoundException() : base("Can't find playlist.")
    {
    }
}