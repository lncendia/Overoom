﻿namespace Watch2gether.Application.Abstractions.Exceptions.Users;

public class UserAlreadyExistException : Exception
{
    public UserAlreadyExistException() : base("The user is already registered.")
    {
    }
}