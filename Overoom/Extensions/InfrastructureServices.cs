using Overoom.Application.Abstractions.Interfaces.Films;
using Overoom.Application.Abstractions.Interfaces.Users;
using Overoom.Infrastructure.Mailing;
using Overoom.Infrastructure.MovieDownloader;
using Overoom.Infrastructure.PhotoManager;

namespace Overoom.Extensions;

public static class InfrastructureServices
{
    public static void AddInfrastructureServices(this IServiceCollection services, string rootPath)
    {
        services.AddScoped<IEmailService, EmailService>(_ =>
            new EmailService("egor.lazeba@yandex.ru", "ilizzhetwisfwirw", "smtp.yandex.ru", 587));

        services.AddScoped<IUserThumbnailService, UserThumbnailService>(
            _ => new UserThumbnailService(Path.Combine(rootPath, "img")));
        services.AddScoped<IFilmPosterService, FilmPosterService>(
            _ => new FilmPosterService(Path.Combine(rootPath, "img")));

        services.AddScoped<IFilmInfoGetterService, FilmGetterService>(_ =>
            new FilmGetterService("6oDZugvTXZogUnTodylqzeEP7c4lmnkd", "e2f56e43-04aa-4388-8852-addef6f31247"));
    }
}