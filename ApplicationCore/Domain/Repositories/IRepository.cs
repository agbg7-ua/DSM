using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories
{
    public interface IRepository<T, TKey> where TKey : struct
    {
        T GetById(TKey id);
        IEnumerable<T> GetAll();
        void New(T entity);
        void Modify(T entity);
        void Destroy(T entity);
    }
}