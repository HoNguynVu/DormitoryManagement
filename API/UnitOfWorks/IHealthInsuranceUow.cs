using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IHealthInsuranceUow : ITransactionManager
    {
        public IHealthInsuranceRepository HealthInsurances { get; }
        public IReceiptRepository Receipts { get; }
        public IStudentRepository Students { get; }

        public IHealthPriceRepository HealthPrices { get; }
    }
}
