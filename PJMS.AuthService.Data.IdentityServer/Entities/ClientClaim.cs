﻿namespace PJMS.AuthService.Data.IdentityServer.Entities;

public class ClientClaim
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; }
}