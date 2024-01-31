using System.Globalization;
using Films.Start.Extensions;
using Films.Start.HostedServices;
using Films.Start.Infrastructure.Storage.Context;
using Films.Start.WEB.Hubs;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU")
{
    NumberFormat = new NumberFormatInfo
    {
        NumberDecimalSeparator = "."
    }
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddApplicationMappers();
builder.Services.AddControllerMappers();
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment.WebRootPath);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddEventHandlers();

builder.Services.AddHostedService<FilmLoadHostedService>();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<FilmRoomHub>("/filmRoom");
app.MapHub<YoutubeRoomHub>("/youtubeRoom");
app.Run();