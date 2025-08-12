using System.Linq.Expressions;

namespace CryptoWallet.Domain.Common;

/// <summary>
/// Base repository interface for data access operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IRepository<TEntity> where TEntity : Entity
{
    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <returns>Queryable collection of entities</returns>
    IQueryable<TEntity> GetAll();

    /// <summary>
    /// Finds entities matching the specified predicate
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition</param>
    /// <returns>Queryable collection of matching entities</returns>
    IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Gets an entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The entity if found; otherwise, null</returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a collection of entities to the repository
    /// </summary>
    /// <param name="entities">The collection of entities to add</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository
    /// </summary>
    /// <param name="entity">The entity to update</param>
    void Update(TEntity entity);

    /// <summary>
    /// Removes an entity from the repository
    /// </summary>
    /// <param name="entity">The entity to remove</param>
    void Remove(TEntity entity);

    /// <summary>
    /// Removes an entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the entity to remove</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a collection of entities from the repository
    /// </summary>
    /// <param name="entities">The collection of entities to remove</param>
    void RemoveRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Determines whether any entity matches the specified condition
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>True if any element matches the condition; otherwise, false</returns>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of entities that match the optional condition
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition (optional)</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of entities that match the condition, or the total count if no condition is provided</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null,
                         CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made in this repository to the underlying data store
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
