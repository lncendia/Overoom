﻿using Microsoft.AspNetCore.Identity;

namespace Overoom.Application.Abstractions.Authentication.Entities;

public class RoleData : IdentityRole
{
    public RoleData(string name) : base(name)
    {
    }
}