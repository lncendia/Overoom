﻿namespace Watch2gether.Application.Abstractions.Exceptions.Users;

public class InvalidCodeException : Exception
{
    public InvalidCodeException() : base("Invalid code specified.")
    {
    }
}