using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IPaymentUow : ITransactionManager
    {
        public IPaymentRepository Payments { get; }
        public IRegistrationFormRepository RegistrationForms { get; }
        public IRoomTypeRepository RoomTypes { get; }
        public IReceiptRepository Receipts { get; }
    }
}
