using System;

namespace ApplicationCore.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        void BeginTransaction();
        void SaveChanges();
        void Rollback();
    }
}