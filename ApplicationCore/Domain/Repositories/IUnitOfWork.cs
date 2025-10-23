namespace ApplicationCore.Domain.Repositories
{
    public interface IUnitOfWork
    {
        void SaveChanges();
    }
}