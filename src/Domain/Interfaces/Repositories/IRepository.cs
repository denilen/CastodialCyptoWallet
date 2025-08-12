using System.Linq.Expressions;
using CryptoWallet.Domain.Common;

namespace CryptoWallet.Domain.Interfaces.Repositories;

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
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an entity from the repository
    /// </summary>
    /// <param name="entity">The entity to remove</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a collection of entities from the repository
    /// </summary>
    /// <param name="entities">The collection of entities to remove</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specified predicate
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>True if any element matches the condition; otherwise, false</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of entities in the repository
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of entities</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of entities that match the specified predicate
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of entities that match the condition</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made in this context to the database
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
