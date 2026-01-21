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
public class UserStore : Incendia.Identity.Mongo.Stores.UserStore<AppUser, AppRole, Guid>
{
  /// <summary>
  ///
  /// </summary>
  private readonly MongoDbContext _context;

  /// <summary>
  ///
  /// </summary>
  /// <param name="userCollection"></param>
  /// <param name="roleCollection"></param>
  /// <param name="describer"></param>
  /// <param name="config"></param>
  /// <param name="context"></param>
  public UserStore(IMongoCollection<MongoUser<Guid, AppUser>> userCollection,
    IMongoCollection<MongoRole<Guid, AppRole>> roleCollection, IdentityErrorDescriber? describer,
    IdentityModelBuilder config, MongoDbContext context) : base(userCollection, roleCollection, describer, config)
  {
    _context = context;
  }

  /// <summary>
  ///
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  protected override Task<BulkWriteResult<MongoUser<Guid, AppUser>>> SaveChangesAsync(CancellationToken cancellationToken)
  {
    return SaveChangesAsync(UserCollection, _context.Session, cancellationToken: cancellationToken);
  }
}
