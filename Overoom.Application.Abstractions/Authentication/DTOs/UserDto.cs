﻿namespace Overoom.Application.Abstractions.Authentication.DTOs;

public class UserDto
{
    public UserDto(string username, string email, Guid id)
    {
        Username = username;
        Email = email;
        Id = id;
    }

    public Guid Id { get; }
    public string Username { get; }
    public string Email { get; }
}