// ReSharper disable ArrangeMethodOrOperatorBody

namespace UnitOfWorkSharp;

using System.Data;
using System.Text.RegularExpressions;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents the default implementation of the <see cref="IUnitOfWork" /> and <see cref="IUnitOfWork{TContext}" />
///     interface.
/// </summary>
/// <typeparam name="TContext">The type of the db context.</typeparam>
public class UnitOfWork<TContext> : IRepositoryFactory, IUnitOfWork<TContext>
    where TContext : DbContext
{
    private readonly Dictionary<Type, object> _repositories;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnitOfWork{TContext}" /> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public UnitOfWork(TContext context)
    {
        DbContext = context ?? throw new ArgumentNullException(nameof(context));
        _repositories = new Dictionary<Type, object>();
    }

    /// <summary>
    ///     Gets the db context.
    /// </summary>
    /// <returns>The instance of type <typeparamref name="TContext" />.</returns>
    public TContext DbContext { get; }

    /// <summary>
    ///     Gets the specified repository for the <typeparamref name="TEntity" />.
    /// </summary>
    /// <param name="hasCustomRepository"><c>True</c> if providing custom repository </param>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>An instance of type inherited from <see cref="IRepository{TEntity}" /> interface.</returns>
    public IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false)
        where TEntity : class
    {
        // what's the best way to support custom repository?
        if (hasCustomRepository)
        {
            var customRepo = DbContext.GetService<IRepository<TEntity>>();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (customRepo != null)
            {
                return customRepo;
            }
        }

        var type = typeof(TEntity);
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new Repository<TEntity>(DbContext);
        }

        return (IRepository<TEntity>)_repositories[type];
    }

    /// <summary>
    ///     Changes the database name. This require the databases in the same machine. NOTE: This only work for MySQL right
    ///     now.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <remarks>
    ///     This only been used for supporting multiple databases in the same model. This require the databases in the same
    ///     machine.
    /// </remarks>
    public void ChangeDatabase(string database)
    {
        var connection = DbContext.Database.GetDbConnection();
        if (connection.State.HasFlag(ConnectionState.Open))
        {
            connection.ChangeDatabase(database);
        }
        else
        {
            var connectionString = Regex.Replace(connection.ConnectionString.Replace(" ", ""), @"(?<=[Dd]atabase=)\w+(?=;)", database, RegexOptions.Singleline);
            connection.ConnectionString = connectionString;
        }

        // Following code only working for mysql.
        var items = DbContext.Model.GetEntityTypes();
        foreach (var item in items)
        {
            if (item is IConventionEntityType entityType)
            {
                entityType.SetSchema(database);
            }
        }
    }

    /// <summary>
    ///     Executes the specified raw SQL command.
    /// </summary>
    /// <param name="sql">The raw SQL.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The number of state entities written to database.</returns>
    public int ExecuteSqlCommand(string sql, params object[] parameters) => DbContext.Database.ExecuteSqlRaw(sql, parameters);

    /// <summary>
    ///     Uses raw SQL queries to fetch the specified <typeparamref name="TEntity" /> data.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="sql">The raw SQL.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>An <see cref="IQueryable{T}" /> that contains elements that satisfy the condition specified by raw SQL.</returns>
    public IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters)
        where TEntity : class =>
        DbContext.Set<TEntity>()
            .FromSqlRaw(sql, parameters);

    /// <summary>
    ///     Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="ensureAutoHistory"><c>True</c> if save changes ensure auto record the change history.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public virtual int SaveChanges(bool ensureAutoHistory = false)
    {
        return ExecuteSaveChangesAsync(ensureAutoHistory)
            .GetAwaiter()
            .GetResult();
    }

    /// <summary>
    ///     Asynchronously saves all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="ensureAutoHistory"><c>True</c> if save changes ensure auto record the change history.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> that represents the asynchronous save operation. The task result contains the
    ///     number of state entities written to database.
    /// </returns>
    public virtual async Task<int> SaveChangesAsync(bool ensureAutoHistory = false, CancellationToken cancellationToken = default)
    {
        return await ExecuteSaveChangesAsync(ensureAutoHistory, cancellationToken);
    }

    /// <summary>
    ///     Saves all changes made in this context to the database with distributed transaction.
    /// </summary>
    /// <param name="ensureAutoHistory"><c>True</c> if save changes ensure auto record the change history.</param>
    /// <param name="unitOfWorks">An optional <see cref="IUnitOfWork" /> array.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> that represents the asynchronous save operation. The task result contains the
    ///     number of state entities written to database.
    /// </returns>
    public virtual async Task<int> SaveChangesAsync(bool ensureAutoHistory = false, params IUnitOfWork[] unitOfWorks)
    {
        using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var count = 0;
        foreach (var unitOfWork in unitOfWorks)
        {
            count += await unitOfWork.SaveChangesAsync(ensureAutoHistory)
                .ConfigureAwait(false);
        }

        count += await SaveChangesAsync(ensureAutoHistory);

        ts.Complete();

        return count;
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    public void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback) => DbContext.ChangeTracker.TrackGraph(rootEntity, callback);

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">The disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // clear repositories
                if (_repositories != null)
                {
                    _repositories.Clear();
                }

                // dispose the db context.
                DbContext.Dispose();
            }
        }

        _disposed = true;
    }

    private async Task<int> ExecuteSaveChangesAsync(bool ensureAutoHistory = false, CancellationToken cancellationToken = default)
    {
        using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (ensureAutoHistory)
        {
            DbContext.EnsureAutoHistory();
        }

        try
        {
            var entitiesChangedCount = await DbContext.SaveChangesAsync(cancellationToken);
            ts.Complete();

            return entitiesChangedCount;
        }
        catch (Exception)
        {
            Dispose();

            throw;
        }
    }
}