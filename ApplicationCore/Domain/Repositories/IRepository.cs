namespace ApplicationCore.Domain.Repositories;

public interface IRepository<TEntity, TKey> where TEntity : class
{
    TEntity? DamePorOID(TKey id);
    IList<TEntity> DameTodos();
    long New(TEntity entity);
    void Modify(TEntity entity);
    void Destroy(TEntity entity);
    void ModifyAll(IEnumerable<TEntity> entities);
}
