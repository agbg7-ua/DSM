using NHibernate;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.Repositories
{
    public class NHibernateUnitOfWork : IUnitOfWork
    {
        private readonly ISession _session;
        private ITransaction? _transaction;

        public NHibernateUnitOfWork(ISession session)
        {
            _session = session;
        }

        public void BeginTransaction()
        {
            if (_transaction?.IsActive ?? false) return;
            _transaction = _session.BeginTransaction();
        }

        public void SaveChanges()
        {
            if (_transaction?.IsActive ?? false)
            {
                _transaction.Commit();
                _transaction = null;
            }
            else
            {
                // Auto-begin transaction if none exists
                BeginTransaction();
                _transaction?.Commit();
                _transaction = null;
            }
        }

        public void Rollback()
        {
            if (_transaction?.IsActive ?? false)
            {
                _transaction.Rollback();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            if (_transaction?.IsActive ?? false)
            {
                _transaction.Rollback();
            }
            _transaction?.Dispose();
            _session.Dispose();
        }
    }
}