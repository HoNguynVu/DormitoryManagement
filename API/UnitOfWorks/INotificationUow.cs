using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface INotificationUow : ITransactionManager
    {
        public INotificationRepository Notifications { get; }
    }
}
