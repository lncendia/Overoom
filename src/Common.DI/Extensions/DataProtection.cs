using System.Security.Cryptography.X509Certificates;
using Common.Infrastructure.DataProtection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Common.DI.Extensions;

/// <summary>
/// Предоставляет методы расширения для интеграции системы защиты данных и MongoDB
/// </summary>
public static class DataProtection
{
  /// <summary>
  /// Настраивает систему защиты данных с хранением ключей и MongoDB
  /// </summary>
  /// <param name="builder">Построитель приложения</param>
  /// <param name="applicationName">Уникальное имя этого приложения в системе защиты данных</param>
  public static void AddSecureDataProtection(this IHostApplicationBuilder builder, string applicationName)
  {
    string certificatePath = builder.Configuration.GetRequiredValue<string>("DataProtection:Certificate:Path");
    string certificatePassword = builder.Configuration.GetRequiredValue<string>("DataProtection:Certificate:Password");

    X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12FromFile(
      certificatePath,
      certificatePassword,
      X509KeyStorageFlags.MachineKeySet |
      X509KeyStorageFlags.Exportable |
      X509KeyStorageFlags.PersistKeySet
    );

    string databaseName = builder.Configuration.GetRequiredValue<string>("MongoDB:DataProtectionDB");

    builder.Services.AddDataProtection()
      .ProtectKeysWithCertificate(certificate)
      .SetApplicationName(applicationName);

    builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(services =>
    {
      IMongoClient mongoClient = services.GetRequiredService<IMongoClient>();
      return new ConfigureOptions<KeyManagementOptions>(options =>
      {
        options.XmlRepository = new MongoDbXmlRepository(mongoClient, databaseName);
      });
    });
  }
}