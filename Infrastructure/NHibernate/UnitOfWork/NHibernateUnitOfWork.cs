using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.UnitOfWork;

public class NHibernateUnitOfWork : IUnitOfWork
{
    private readonly ISession _session;
    private ITransaction? _transaction;

    public NHibernateUnitOfWork(ISession session)
    {
        _session = session;
        _transaction = _session.BeginTransaction();
    }

    public void SaveChanges()
    {
        if (_transaction is null)
            _transaction = _session.BeginTransaction();

        _transaction.Commit();
        _transaction = _session.BeginTransaction();
    }
}
