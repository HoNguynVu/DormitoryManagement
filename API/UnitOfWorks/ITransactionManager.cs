namespace API.UnitOfWorks
{
    public interface ITransactionManager
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
