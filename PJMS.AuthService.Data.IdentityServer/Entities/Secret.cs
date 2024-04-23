﻿namespace PJMS.AuthService.Data.IdentityServer.Entities;

public abstract class Secret
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string Value { get; set; }
    public DateTime? Expiration { get; set; }
    public string Type { get; set; } = "SharedSecret";
    public DateTime Created { get; set; } = DateTime.UtcNow;
}