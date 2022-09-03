﻿namespace Watch2gether.Application.Abstractions.Exceptions.Users;

public class ThumbnailSaveException : Exception
{
    public ThumbnailSaveException(Exception ex) : base("Failed to save thumbnail", ex)
    {
    }
}