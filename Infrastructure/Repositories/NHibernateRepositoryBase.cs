using NHibernate;
using System.Collections.Generic;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.Repositories
{
    public class NHibernateRepositoryBase<T> : IRepository<T, long> where T : class
    {
        protected readonly ISession _session;

        public NHibernateRepositoryBase(ISession session)
        {
            _session = session;
        }

        public virtual T GetById(long id) => _session.Get<T>(id);

        public virtual IEnumerable<T> GetAll() => _session.QueryOver<T>().List();

        public virtual void New(T entity) => _session.Save(entity);

        public virtual void Modify(T entity) => _session.Update(entity);

        public virtual void Destroy(T entity) => _session.Delete(entity);
    }
}
