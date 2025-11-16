using System;
using System.Threading.Tasks;
using ImageManager.Data;                // <-- your DbContext namespace
using Microsoft.EntityFrameworkCore.Storage;

namespace ImageManager.Services;

/// <summary>
/// A tiny abstraction that gives us a single entry point for starting a transaction.
/// </summary>
public interface ITransactionService
{
    /// <summary>Starts a new EF Core database transaction.</summary>
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task SaveChangesAsync();
    Task SaveChangesAsync(CancellationToken cancellationToken);
    IDbContextTransaction? CurrentTransaction { get; }
}

/// <summary>
/// Concrete implementation that simply forwards to the DbContext.
/// </summary>
public class TransactionService(ApplicationDbContext context) : ITransactionService
{
    
    public Task SaveChangesAsync() => context.SaveChangesAsync();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
       
        var existing = context.Database.CurrentTransaction;
        if (existing != null)
            return existing;          

        // Otherwise start a brand‑new transaction
        return await context.Database.BeginTransactionAsync();
    }
    
    public IDbContextTransaction? CurrentTransaction => context.Database.CurrentTransaction;
}

/// <summary>
/// Extension helpers that let you write “use a transaction” in one line.
/// </summary>
public static class TransactionServiceExtensions
{
    /// <summary>
    /// Executes the supplied async action inside a transaction.  
    /// If the action completes successfully, the transaction is committed; otherwise it is rolled back.
    /// </summary>
    public static async Task UseTransactionAsync(this ITransactionService tx,
                                                Func<Task> action)
    {
        await using var transaction = await tx.BeginTransactionAsync();
        try
        {
            await action();                    
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Same as above, but returns a value from the action.
    /// </summary>
    public static async Task<T> UseTransactionAsync<T>(this ITransactionService tx,
                                                       Func<Task<T>> action)
    {
        await using var transaction = await tx.BeginTransactionAsync();
        try
        {
            T result = await action();         
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Optional overload for a synchronous Action – it just wraps the call in Task.Run.
    /// </summary>
    public static async Task UseTransactionAsync(this ITransactionService tx,
                                                Action action)
        => await UseTransactionAsync(tx, () => { action(); return Task.CompletedTask; });
}
