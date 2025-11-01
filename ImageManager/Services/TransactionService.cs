using ImageManager.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace ImageManager.Services;

public interface ITransactionService
{
    Task<IDbContextTransaction> BeginTransactionAsync();
}

public class TransactionService(ApplicationDbContext context) : ITransactionService
{
    public Task<IDbContextTransaction> BeginTransactionAsync() => context.Database.BeginTransactionAsync();
}