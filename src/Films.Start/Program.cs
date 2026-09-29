using System.Text.Json.Serialization;
using Common.DI.Extensions;
using Common.DI.Filters;
using Common.DI.Middlewares;
using Common.DI.WebApi.Extensions;
using Films.Application.Abstractions;
using Films.Application.Abstractions.DTOs.Films;
using Films.Application.Services.CommandHandlers.Ratings;
using Films.Infrastructure.Storage.DatabaseInitialization;
using Films.Infrastructure.Storage.EventHandlers;
using Films.Infrastructure.Web.Films.Controllers;
using Films.Infrastructure.Web.Films.Mappers;
using Films.Infrastructure.Web.Films.Validators;
using Films.Start.Exceptions;
using Films.Start.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.InitializeMongoDb();
builder.AddLoggingServices();
builder.AddJwtAuthentication();
builder.AddSwaggerServices(typeof(FilmsController), typeof(FilmDto));
builder.AddFileStorage();
builder.AddStorageServices();
builder.AddMassTransitServices();
builder.AddCorsServices();
builder.Services.AddFileStorageHttpClient();
builder.Services.AddAuthorizationPolicies();
builder.Services.AddValidationServices(typeof(SearchFilmsValidator));
builder.Services.AddMediatorServices(typeof(SetRatingCommandHandler), typeof(RatingCreatedStatisticsEventHandler));
builder.Services.AddMappingServices(typeof(FilmsMapperProfile));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandler>();

builder.Services.AddControllers(options => options.Filters.Add<TransactionFilter>()).AddJsonOptions(options =>
{
  options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});

builder.Services.AddOpenTelemetryServices(Constants.OpenTelemetry.ServiceName);
WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
  await DatabaseInitializer.InitAsync(scope.ServiceProvider);
}

app.UseExceptionHandler();
app.UseCors(CorsServices.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseAuthorizedSwaggerUI();
app.MapControllers();
app.MapPrometheusScrapingEndpointWithBasicAuth();
await app.RunAsync();