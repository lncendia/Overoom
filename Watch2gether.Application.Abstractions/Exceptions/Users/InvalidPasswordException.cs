﻿namespace Watch2gether.Application.Abstractions.Exceptions.Users;

public class InvalidPasswordException : Exception
{
    public InvalidPasswordException() : base("Invalid password entered.")
    {
    }
}