using Films.Application.Abstractions.Posters;
using Films.Domain.Abstractions.Interfaces;
using Films.Infrastructure.Storage;
using Films.Infrastructure.Storage.Context;
using Films.Infrastructure.Storage.Posters;
using Microsoft.EntityFrameworkCore;

namespace Films.Start.Extensions;

public static class PersistenceServices
{
    public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration,
        string rootPath)
    {
        var connectionString = configuration.GetRequiredValue<string>("ConnectionStrings:DefaultConnection");

        var contentPath = configuration.GetRequiredValue<string>("Thumbnails");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString,
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        });
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPosterStore, PosterStore>(_ => new PosterStore(rootPath, contentPath));
    }
}