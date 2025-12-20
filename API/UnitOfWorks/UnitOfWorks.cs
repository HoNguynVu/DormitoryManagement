using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore.Storage;
using DataAccess.Repository;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace API.UnitOfWorks
{
    public class UnitOfWork : IAuthUow, IRegistrationUow, IViolationUow, IRoomUow,
        IContractUow, IPaymentUow , IHealthInsuranceUow, IParameterUow, IUtilityBillUow, IMaintenanceUow, IBuildingUow,
        IPublicInformationUow, IStudentUow, IRoomTypeUow
    {
        private readonly DormitoryDbContext _context;
        private IDbContextTransaction? _transaction;

        public IAccountRepository Accounts { get; }
        public IRefreshTokenRepository RefreshTokens { get; }
        public IOtpRepository OtpCodes { get; }
        public IStudentRepository Students { get; }
        public IRegistrationFormRepository RegistrationForms { get; }
        public IContractRepository Contracts { get; }
        public IRoomRepository Rooms { get; }
        public IRoomTypeRepository RoomTypes { get; }
        public IViolationRepository Violations { get; }
        public IPaymentRepository Payments { get; }
        public IReceiptRepository Receipts { get; }
        public IHealthInsuranceRepository HealthInsurances { get; }
        public IParameterRepository Parameters { get; }
        public IUtilityBillRepository UtilityBills { get; }
        public INotificationRepository Notifications { get; }
        public IPriorityRepository Priorities { get; } 
        public ISchoolRepository Schools { get; }
        public IMaintenanceRepository Maintenances { get; }
        public IEquipmentRepository Equipments { get; }
        public IBuildingManagerRepository BuildingManagers { get; }
        public IBuildingRepository Buildings { get; }
        public UnitOfWork(DormitoryDbContext context, IDbContextTransaction? dbContextTransaction)
        {
            _context = context;
            _transaction = dbContextTransaction;


            Accounts = new AccountRepository(_context);
            Violations = new ViolationRepository(_context);
            Contracts = new ContractRepository(_context);
            Students = new StudentRepository(_context);
            OtpCodes = new OtpRepository(_context);
            RefreshTokens = new RefreshTokenRepository(_context);
            RegistrationForms = new RegistrationFormRepository(_context);
            Rooms = new RoomRepository(_context);
            RoomTypes = new RoomTypeRepository(_context);
            Payments = new PaymentRepository(_context);
            Receipts = new ReceiptRepository(_context);
            HealthInsurances = new HealthInsuranceRepository(_context);
            Maintenances = new MaintenanceRepository(_context);
            Parameters = new ParameterRepository(_context);
            UtilityBills = new UtilityBillRepository(_context);
            Notifications = new NotificationRepository(_context);
            Equipments = new EquipmentRepository(_context);
            BuildingManagers = new BuildingManagerRepository(_context);
            Priorities = new PriorityRepository(_context);
            Schools = new SchoolRepository(_context);
            Buildings = new BuildingRepository(_context);
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction == null)
            {
                // Truyền isolationLevel vào hàm của EF Core
                _transaction = await _context.Database.BeginTransactionAsync(isolationLevel);
            }
        }

        // Triển khai các hàm Transaction
        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
        }
        public async Task CommitAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Transaction not started");

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // Persist non-transactional changes
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> BuildingExistsAsync(string buildingId)
        {
            return await _context.Buildings.AnyAsync(b => b.BuildingID == buildingId);
        }
    }
}
