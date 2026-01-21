using System;
using System.Threading;
using System.Threading.Tasks;

using Identix.Application.Abstractions.Entities;

using Incendia.Identity.Mongo;
using Incendia.Identity.Mongo.Model;

using MassTransit.MongoDbIntegration;

using Microsoft.AspNetCore.Identity;

using MongoDB.Driver;

namespace Identix.Infrastructure.Common.Identity.Stores;

/// <summary>
///
/// </summary>
public class RoleStore : Incendia.Identity.Mongo.Stores.RoleStore<AppRole, Guid>
{
  /// <summary>
  ///
  /// </summary>
  private readonly MongoDbContext _context;

  /// <summary>
  ///
  /// </summary>
  /// <param name="roleCollection"></param>
  /// <param name="describer"></param>
  /// <param name="config"></param>
  /// <param name="context"></param>
  public RoleStore(IMongoCollection<MongoRole<Guid, AppRole>> roleCollection, IdentityErrorDescriber? describer,
    IdentityModelBuilder config, MongoDbContext context) : base(roleCollection, describer, config)
  {
    _context = context;
  }

  /// <summary>
  ///
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  protected override Task<BulkWriteResult<MongoRole<Guid, AppRole>>> SaveChangesAsync(CancellationToken cancellationToken)
  {
    return SaveChangesAsync(Collection, _context.Session, cancellationToken: cancellationToken);
  }
}
