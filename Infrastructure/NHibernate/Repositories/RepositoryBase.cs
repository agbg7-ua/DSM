using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories;

public abstract class RepositoryBase<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly ISession Session;

    protected RepositoryBase(ISession session)
    {
        Session = session;
    }

    public TEntity? DamePorOID(TKey id) => Session.Get<TEntity>(id);

    public IList<TEntity> DameTodos() => Session.Query<TEntity>().ToList();

    public long New(TEntity entity)
    {
        var id = Session.Save(entity);
        return Convert.ToInt64(id);
    }

    public void Modify(TEntity entity) => Session.Update(entity);

    public void Destroy(TEntity entity) => Session.Delete(entity);

    public void ModifyAll(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            Session.Update(entity);
        }
    }
}
