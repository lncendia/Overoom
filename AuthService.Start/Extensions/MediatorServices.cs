using AuthService.Application.Services.Commands.Create;

namespace AuthService.Start.Extensions;

public static class MediatorServices
{
    public static void AddMediatorServices(this IServiceCollection services)
    {
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(CreateUserCommandHandler).Assembly));
    }
}