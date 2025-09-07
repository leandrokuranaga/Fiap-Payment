using Fiap.Payment.Domain.SeedWork;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Fiap.Payment.Infra.Data
{
    public class UnitOfWork(Context context, ILogger<UnitOfWork> logger) : IUnitOfWork, IDisposable
    {
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        public async Task BeginTransactionAsync()
        {
            if (_transaction is not null) return;
            _transaction = await context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction is not null)
            {
                try
                {
                    await context.SaveChangesAsync();
                    await _transaction.CommitAsync();
                }
                catch
                {
                    await _transaction.RollbackAsync();
                    throw;
                }
                finally
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            else
            {
                await context.SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }

        public async Task RollbackAsync()
        {
            if (_transaction is not null)
            {
                try
                {
                    await _transaction.RollbackAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError("Error trying to rollback: {Message}", ex.Message);
                }
                finally
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _transaction?.Dispose();
                context.Dispose();
                _disposed = true;
            }
        }
    }
}
