using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.MongoDbIntegration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Search;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Identix.Infrastructure.Common.Mongo;

public class OutboxMongoCollection<T>(IMongoCollection<T> innerCollection, MongoDbContext mongoDbContext)
    : IMongoCollection<T>
{
    public IAsyncCursor<TResult> Aggregate<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? Aggregate(mongoDbContext.Session, pipeline, options, cancellationToken)
            : innerCollection.Aggregate(pipeline, options, cancellationToken);

    public IAsyncCursor<TResult> Aggregate<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline,
        AggregateOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.Aggregate(session, pipeline, options, cancellationToken);

    public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? AggregateAsync(mongoDbContext.Session, pipeline, options, cancellationToken)
            : innerCollection.AggregateAsync(pipeline, options, cancellationToken);

    public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.AggregateAsync(session, pipeline, options, cancellationToken);

    public void AggregateToCollection<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null,
        CancellationToken cancellationToken = new())
    {
        if (mongoDbContext.Session != null)
            AggregateToCollection(mongoDbContext.Session, pipeline, options, cancellationToken);
        else
            innerCollection.AggregateToCollection(pipeline, options, cancellationToken);
    }

    public void AggregateToCollection<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline,
        AggregateOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.AggregateToCollection(session, pipeline, options, cancellationToken);

    public Task AggregateToCollectionAsync<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? AggregateToCollectionAsync(mongoDbContext.Session, pipeline, options, cancellationToken)
            : innerCollection.AggregateToCollectionAsync(pipeline, options, cancellationToken);

    public Task AggregateToCollectionAsync<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline,
        AggregateOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.AggregateToCollectionAsync(session, pipeline, options, cancellationToken);

    public BulkWriteResult<T> BulkWrite(IEnumerable<WriteModel<T>> requests, BulkWriteOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? BulkWrite(mongoDbContext.Session, requests, options, cancellationToken)
            : innerCollection.BulkWrite(requests, options, cancellationToken);

    public BulkWriteResult<T> BulkWrite(IClientSessionHandle session, IEnumerable<WriteModel<T>> requests, BulkWriteOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.BulkWrite(session, requests, options, cancellationToken);

    public Task<BulkWriteResult<T>> BulkWriteAsync(IEnumerable<WriteModel<T>> requests, BulkWriteOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? BulkWriteAsync(mongoDbContext.Session, requests, options, cancellationToken)
            : innerCollection.BulkWriteAsync(requests, options, cancellationToken);

    public Task<BulkWriteResult<T>> BulkWriteAsync(IClientSessionHandle session, IEnumerable<WriteModel<T>> requests, BulkWriteOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.BulkWriteAsync(session, requests, options, cancellationToken);

    public long Count(FilterDefinition<T> filter, CountOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? Count(mongoDbContext.Session, filter, options, cancellationToken)
            : innerCollection.Count(filter, options, cancellationToken);

    public long Count(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.Count(session, filter, options, cancellationToken);

    public Task<long> CountAsync(FilterDefinition<T> filter, CountOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? CountAsync(mongoDbContext.Session, filter, options, cancellationToken)
            : innerCollection.CountAsync(filter, options, cancellationToken);

    public Task<long> CountAsync(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.CountAsync(session, filter, options, cancellationToken);

    public long CountDocuments(FilterDefinition<T> filter, CountOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? CountDocuments(mongoDbContext.Session, filter, options, cancellationToken)
            : innerCollection.CountDocuments(filter, options, cancellationToken);

    public long CountDocuments(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.CountDocuments(session, filter, options, cancellationToken);

    public Task<long> CountDocumentsAsync(FilterDefinition<T> filter, CountOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? CountDocumentsAsync(mongoDbContext.Session, filter, options, cancellationToken)
            : innerCollection.CountDocumentsAsync(filter, options, cancellationToken);

    public Task<long> CountDocumentsAsync(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.CountDocumentsAsync(session, filter, options, cancellationToken);

    public DeleteResult DeleteMany(FilterDefinition<T> filter, CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? DeleteMany(mongoDbContext.Session, filter, null, cancellationToken)
            : innerCollection.DeleteMany(filter, cancellationToken);

    public DeleteResult DeleteMany(FilterDefinition<T> filter, DeleteOptions options,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? DeleteMany(mongoDbContext.Session, filter, options, cancellationToken)
            : innerCollection.DeleteMany(filter, options, cancellationToken);

    public DeleteResult DeleteMany(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.DeleteMany(session, filter, options, cancellationToken);

    public Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? DeleteManyAsync(mongoDbContext.Session, filter, null, cancellationToken)
            : innerCollection.DeleteManyAsync(filter, cancellationToken);

    public Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter, DeleteOptions options,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? DeleteManyAsync(mongoDbContext.Session, filter, options, cancellationToken)
            : innerCollection.DeleteManyAsync(filter, options, cancellationToken);

    public Task<DeleteResult> DeleteManyAsync(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.DeleteManyAsync(session, filter, options, cancellationToken);

    public DeleteResult DeleteOne(FilterDefinition<T> filter, CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? DeleteOne(mongoDbContext.Session, filter, null, cancellationToken)
            : innerCollection.DeleteOne(filter, cancellationToken);

    public DeleteResult DeleteOne(FilterDefinition<T> filter, DeleteOptions options,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? DeleteOne(mongoDbContext.Session, filter, options, cancellationToken)
            : innerCollection.DeleteOne(filter, options, cancellationToken);

    public DeleteResult DeleteOne(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.DeleteOne(session, filter, options, cancellationToken);

    public Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? DeleteOneAsync(mongoDbContext.Session, filter, null, cancellationToken)
            : innerCollection.DeleteOneAsync(filter, cancellationToken);

    public Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter, DeleteOptions options,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? DeleteOneAsync(mongoDbContext.Session, filter, options, cancellationToken)
            : innerCollection.DeleteOneAsync(filter, options, cancellationToken);

    public Task<DeleteResult> DeleteOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.DeleteOneAsync(session, filter, options, cancellationToken);

    public IAsyncCursor<TField> Distinct<TField>(FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? Distinct(mongoDbContext.Session, field, filter, options, cancellationToken)
            : innerCollection.Distinct(field, filter, options, cancellationToken);

    public IAsyncCursor<TField> Distinct<TField>(IClientSessionHandle session, FieldDefinition<T, TField> field, FilterDefinition<T> filter,
        DistinctOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.Distinct(session, field, filter, options, cancellationToken);

    public Task<IAsyncCursor<TField>> DistinctAsync<TField>(FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? DistinctAsync(mongoDbContext.Session, field, filter, options, cancellationToken)
            : innerCollection.DistinctAsync(field, filter, options, cancellationToken);

    public Task<IAsyncCursor<TField>> DistinctAsync<TField>(IClientSessionHandle session, FieldDefinition<T, TField> field, FilterDefinition<T> filter,
        DistinctOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.DistinctAsync(session, field, filter, options, cancellationToken);

    public IAsyncCursor<TItem> DistinctMany<TItem>(FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? DistinctMany(mongoDbContext.Session, field, filter, options, cancellationToken)
            : innerCollection.DistinctMany(field, filter, options, cancellationToken);

    public IAsyncCursor<TItem> DistinctMany<TItem>(IClientSessionHandle session, FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter,
        DistinctOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.DistinctMany(session, field, filter, options, cancellationToken);

    public Task<IAsyncCursor<TItem>> DistinctManyAsync<TItem>(FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? DistinctManyAsync(mongoDbContext.Session, field, filter, options, cancellationToken)
            : innerCollection.DistinctManyAsync(field, filter, options, cancellationToken);

    public Task<IAsyncCursor<TItem>> DistinctManyAsync<TItem>(IClientSessionHandle session, FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter,
        DistinctOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.DistinctManyAsync(session, field, filter, options, cancellationToken);

    public long EstimatedDocumentCount(EstimatedDocumentCountOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.EstimatedDocumentCount(options, cancellationToken);

    public Task<long> EstimatedDocumentCountAsync(EstimatedDocumentCountOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.EstimatedDocumentCountAsync(options, cancellationToken);

    public IAsyncCursor<TProjection> FindSync<TProjection>(FilterDefinition<T> filter, FindOptions<T, TProjection>? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? FindSync(mongoDbContext.Session, filter, options, cancellationToken)
            : innerCollection.FindSync(filter, options, cancellationToken);

    public IAsyncCursor<TProjection> FindSync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOptions<T, TProjection>? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.FindSync(session, filter, options, cancellationToken);

    public Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(FilterDefinition<T> filter, FindOptions<T, TProjection>? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? FindAsync(mongoDbContext.Session, filter, options, cancellationToken)
            : innerCollection.FindAsync(filter, options, cancellationToken);

    public Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOptions<T, TProjection>? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.FindAsync(session, filter, options, cancellationToken);

    public TProjection FindOneAndDelete<TProjection>(FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection>? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? FindOneAndDelete(mongoDbContext.Session, filter, options, cancellationToken)
            : innerCollection.FindOneAndDelete(filter, options, cancellationToken);

    public TProjection FindOneAndDelete<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter,
        FindOneAndDeleteOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => innerCollection.FindOneAndDelete(session, filter, options, cancellationToken);

    public Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection>? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? FindOneAndDeleteAsync(mongoDbContext.Session, filter, options, cancellationToken)
            : innerCollection.FindOneAndDeleteAsync(filter, options, cancellationToken);

    public Task<TProjection> FindOneAndDeleteAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter,
        FindOneAndDeleteOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => innerCollection.FindOneAndDeleteAsync(session, filter, options, cancellationToken);

    public TProjection FindOneAndReplace<TProjection>(FilterDefinition<T> filter, T replacement,
        FindOneAndReplaceOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? FindOneAndReplace(mongoDbContext.Session, filter, replacement, options, cancellationToken)
            : innerCollection.FindOneAndReplace(filter, replacement, options, cancellationToken);

    public TProjection FindOneAndReplace<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, T replacement,
        FindOneAndReplaceOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => innerCollection.FindOneAndReplace(session, filter, replacement, options, cancellationToken);

    public Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<T> filter, T replacement,
        FindOneAndReplaceOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? FindOneAndReplaceAsync(mongoDbContext.Session, filter, replacement, options, cancellationToken)
            : innerCollection.FindOneAndReplaceAsync(filter, replacement, options, cancellationToken);

    public Task<TProjection> FindOneAndReplaceAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, T replacement,
        FindOneAndReplaceOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => innerCollection.FindOneAndReplaceAsync(session, filter, replacement, options, cancellationToken);

    public TProjection FindOneAndUpdate<TProjection>(FilterDefinition<T> filter, UpdateDefinition<T> update,
        FindOneAndUpdateOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? FindOneAndUpdate(mongoDbContext.Session, filter, update, options, cancellationToken)
            : innerCollection.FindOneAndUpdate(filter, update, options, cancellationToken);

    public TProjection FindOneAndUpdate<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter,
        UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection>? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.FindOneAndUpdate(session, filter, update, options, cancellationToken);

    public Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<T> filter, UpdateDefinition<T> update,
        FindOneAndUpdateOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? FindOneAndUpdateAsync(mongoDbContext.Session, filter, update, options, cancellationToken)
            : innerCollection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);

    public Task<TProjection> FindOneAndUpdateAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update,
        FindOneAndUpdateOptions<T, TProjection>? options = null, CancellationToken cancellationToken = new())
        => innerCollection.FindOneAndUpdateAsync(session, filter, update, options, cancellationToken);

    public void InsertOne(T document, InsertOneOptions? options = null,
        CancellationToken cancellationToken = new())
    {
        if (mongoDbContext.Session != null)
            InsertOne(mongoDbContext.Session, document, options, cancellationToken);
        else
            innerCollection.InsertOne(document, options, cancellationToken);
    }

    public void InsertOne(IClientSessionHandle session, T document, InsertOneOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.InsertOne(session, document, options, cancellationToken);

    public Task InsertOneAsync(T document, CancellationToken cancellationToken)
        => mongoDbContext.Session != null
            ? InsertOneAsync(mongoDbContext.Session, document, null, cancellationToken)
            : innerCollection.InsertOneAsync(document, cancellationToken);

    public Task InsertOneAsync(T document, InsertOneOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? InsertOneAsync(mongoDbContext.Session, document, options, cancellationToken)
            : innerCollection.InsertOneAsync(document, options, cancellationToken);

    public Task InsertOneAsync(IClientSessionHandle session, T document, InsertOneOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.InsertOneAsync(session, document, options, cancellationToken);

    public void InsertMany(IEnumerable<T> documents, InsertManyOptions? options = null,
        CancellationToken cancellationToken = new())
    {
        if (mongoDbContext.Session != null)
            InsertMany(mongoDbContext.Session, documents, options, cancellationToken);
        else
            innerCollection.InsertMany(documents, options, cancellationToken);
    }

    public void InsertMany(IClientSessionHandle session, IEnumerable<T> documents, InsertManyOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.InsertMany(session, documents, options, cancellationToken);

    public Task InsertManyAsync(IEnumerable<T> documents, InsertManyOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? InsertManyAsync(mongoDbContext.Session, documents, options, cancellationToken)
            : innerCollection.InsertManyAsync(documents, options, cancellationToken);

    public Task InsertManyAsync(IClientSessionHandle session, IEnumerable<T> documents, InsertManyOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.InsertManyAsync(session, documents, options, cancellationToken);

    public IAsyncCursor<TResult> MapReduce<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult>? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? MapReduce(mongoDbContext.Session, map, reduce, options, cancellationToken)
            : innerCollection.MapReduce(map, reduce, options, cancellationToken);

    public IAsyncCursor<TResult> MapReduce<TResult>(IClientSessionHandle session, BsonJavaScript map, BsonJavaScript reduce,
        MapReduceOptions<T, TResult>? options = null, CancellationToken cancellationToken = new())
        => innerCollection.MapReduce(session, map, reduce, options, cancellationToken);

    public Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult>? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? MapReduceAsync(mongoDbContext.Session, map, reduce, options, cancellationToken)
            : innerCollection.MapReduceAsync(map, reduce, options, cancellationToken);

    public Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(IClientSessionHandle session, BsonJavaScript map, BsonJavaScript reduce,
        MapReduceOptions<T, TResult>? options = null, CancellationToken cancellationToken = new())
        => innerCollection.MapReduceAsync(session, map, reduce, options, cancellationToken);

    public IFilteredMongoCollection<TDerivedDocument> OfType<TDerivedDocument>() where TDerivedDocument : T
        => innerCollection.OfType<TDerivedDocument>();

    public ReplaceOneResult ReplaceOne(FilterDefinition<T> filter, T replacement, ReplaceOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? ReplaceOne(mongoDbContext.Session, filter, replacement, options, cancellationToken)
            : innerCollection.ReplaceOne(filter, replacement, options, cancellationToken);

    public ReplaceOneResult ReplaceOne(FilterDefinition<T> filter, T replacement, UpdateOptions options,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? ReplaceOne(mongoDbContext.Session, filter, replacement, options, cancellationToken)
            : innerCollection.ReplaceOne(filter, replacement, options, cancellationToken);

    public ReplaceOneResult ReplaceOne(IClientSessionHandle session, FilterDefinition<T> filter, T replacement,
        ReplaceOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.ReplaceOne(session, filter, replacement, options, cancellationToken);

    public ReplaceOneResult ReplaceOne(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, UpdateOptions options,
        CancellationToken cancellationToken = new())
        => innerCollection.ReplaceOne(session, filter, replacement, options, cancellationToken);

    public Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement, ReplaceOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? ReplaceOneAsync(mongoDbContext.Session, filter, replacement, options, cancellationToken)
            : innerCollection.ReplaceOneAsync(filter, replacement, options, cancellationToken);

    public Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement, UpdateOptions options,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? ReplaceOneAsync(mongoDbContext.Session, filter, replacement, options, cancellationToken)
            : innerCollection.ReplaceOneAsync(filter, replacement, options, cancellationToken);

    public Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, T replacement,
        ReplaceOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.ReplaceOneAsync(session, filter, replacement, options, cancellationToken);

    public Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, UpdateOptions options,
        CancellationToken cancellationToken = new())
        => innerCollection.ReplaceOneAsync(session, filter, replacement, options, cancellationToken);

    public UpdateResult UpdateMany(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? UpdateMany(mongoDbContext.Session, filter, update, options, cancellationToken)
            : innerCollection.UpdateMany(filter, update, options, cancellationToken);

    public UpdateResult UpdateMany(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update,
        UpdateOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.UpdateMany(session, filter, update, options, cancellationToken);

    public Task<UpdateResult> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? UpdateManyAsync(mongoDbContext.Session, filter, update, options, cancellationToken)
            : innerCollection.UpdateManyAsync(filter, update, options, cancellationToken);

    public Task<UpdateResult> UpdateManyAsync(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update,
        UpdateOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.UpdateManyAsync(session, filter, update, options, cancellationToken);

    public UpdateResult UpdateOne(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? UpdateOne(mongoDbContext.Session, filter, update, options, cancellationToken)
            : innerCollection.UpdateOne(filter, update, options, cancellationToken);

    public UpdateResult UpdateOne(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update,
        UpdateOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.UpdateOne(session, filter, update, options, cancellationToken);

    public Task<UpdateResult> UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? UpdateOneAsync(mongoDbContext.Session, filter, update, options, cancellationToken)
            : innerCollection.UpdateOneAsync(filter, update, options, cancellationToken);

    public Task<UpdateResult> UpdateOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update,
        UpdateOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.UpdateOneAsync(session, filter, update, options, cancellationToken);

    public IChangeStreamCursor<TResult> Watch<TResult>(PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? Watch(mongoDbContext.Session, pipeline, options, cancellationToken)
            : innerCollection.Watch(pipeline, options, cancellationToken);

    public IChangeStreamCursor<TResult> Watch<TResult>(IClientSessionHandle session, PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline,
        ChangeStreamOptions? options = null, CancellationToken cancellationToken = new())
        => innerCollection.Watch(session, pipeline, options, cancellationToken);

    public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = new())
        => mongoDbContext.Session != null
            ? WatchAsync(mongoDbContext.Session, pipeline, options, cancellationToken)
            : innerCollection.WatchAsync(pipeline, options, cancellationToken);

    public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(IClientSessionHandle session, PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = new())
        => innerCollection.WatchAsync(session, pipeline, options, cancellationToken);

    public IMongoCollection<T> WithReadConcern(ReadConcern readConcern)
        => innerCollection.WithReadConcern(readConcern);

    public IMongoCollection<T> WithReadPreference(ReadPreference readPreference)
        => innerCollection.WithReadPreference(readPreference);

    public IMongoCollection<T> WithWriteConcern(WriteConcern writeConcern)
        => innerCollection.WithWriteConcern(writeConcern);

    public CollectionNamespace CollectionNamespace => innerCollection.CollectionNamespace;
    public IMongoDatabase Database => innerCollection.Database;
    public IBsonSerializer<T> DocumentSerializer => innerCollection.DocumentSerializer;
    public IMongoIndexManager<T> Indexes => innerCollection.Indexes;
    public IMongoSearchIndexManager SearchIndexes => innerCollection.SearchIndexes;
    public MongoCollectionSettings Settings => innerCollection.Settings;
}