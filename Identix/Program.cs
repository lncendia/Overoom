using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Identix.Infrastructure.Common.DatabaseInitialization;
using Identix.Extensions;
using Common.DI.Extensions;
using Common.DI.Middlewares;
using Identix.Application.Abstractions;
using Identix.Application.Services.Commands.Create;

// ╔══╗╔═══╗╔═══╗╔═╗ ╔╗╔════╗╔══╗╔═╗╔═╗
// ╚╣╠╝╚╗╔╗║║╔══╝║║╚╗║║║╔╗╔╗║╚╣╠╝╚╗╚╝╔╝
//  ║║  ║║║║║╚══╗║╔╗╚╝║╚╝║║╚╝ ║║  ╚╗╔╝ 
//  ║║  ║║║║║╔══╝║║╚╗║║  ║║   ║║  ╔╝╚╗ 
// ╔╣╠╗╔╝╚╝║║╚══╗║║ ║║║ ╔╝╚╗ ╔╣╠╗╔╝╔╗╚╗
// ╚══╝╚═══╝╚═══╝╚╝ ╚═╝ ╚══╝ ╚══╝╚═╝╚═╝

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.InitializeMongoDb();
builder.AddSecureDataProtection(builder.Environment.ApplicationName);
builder.AddLoggingServices();
builder.AddFileStorage();
builder.AddMongoCache();
builder.AddEmailService();
builder.AddCorsServices();

builder.Services.AddControllersWithViews()

  .AddDataAnnotationsLocalization()

  .AddRazorRuntimeCompilation()

  .AddViewLocalization();

builder.Services.AddFileStorageHttpClient();
builder.AddAspIdentity();
builder.AddOpenId();
builder.Services.AddLocalizationServices();
builder.Services.AddMediatorServices(typeof(CreateUserCommandHandler));
builder.AddMassTransitServices();
builder.AddEmailTemplates();
builder.Services.AddOpenTelemetryServices(Constants.OpenTelemetry.ServiceName);
await using WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
  await DatabaseInitializer.InitAsync(scope.ServiceProvider, builder.Configuration);
}

app.UseRequestLocalization();
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseCors(CorsServices.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapDefaultControllerRoute();
app.MapPrometheusScrapingEndpointWithBasicAuth();
await app.RunAsync();