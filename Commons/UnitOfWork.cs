using System.Data;
using System.Data.Common;
    
namespace mar_sumaken_web.Models.common
{
    public interface IUnitOfWork : IDisposable
    {
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }
        void Begin();
        void Commit();
        void Rollback();
        Task BeginAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private DbConnection _connection;
        private DbTransaction _transaction;
        public UnitOfWork(DbConnection connection)
        {
            _connection = connection;
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }
        public IDbConnection Connection => _connection;

        public IDbTransaction Transaction => _transaction;

        public void Begin()
        {
            _transaction = _connection.BeginTransaction();
        }

        public async Task BeginAsync()
        {
            _transaction = (DbTransaction)await _connection.BeginTransactionAsync();
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public async Task CommitAsync()
        {
            await _transaction.CommitAsync();
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
        }
    }
}
