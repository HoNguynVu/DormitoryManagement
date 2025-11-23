using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore.Storage;
using DataAccess.Repository;

namespace API.UnitOfWorks
{
    public class UnitOfWork : IAuthUow, IViolationUow
    {
        private readonly DormitoryDbContext _context;
        private IDbContextTransaction? _transaction;

        public IAccountRepository Accounts { get; }
        public IRefreshTokenRepository RefreshTokens { get; }
        public IOtpRepository OtpCodes { get; }
        public IStudentRepository Students { get; }

        public IViolationRepository Violations { get; }
        public IContractRepository Contracts { get; }
        public UnitOfWork(DormitoryDbContext context, IDbContextTransaction? dbContextTransaction)
        {
            _context = context;
            _transaction = dbContextTransaction;


            Accounts = new AccountRepository(_context);
            Violations = new ViolationRepository(_context);
            Contracts = new ContractRepository(_context);

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
    }
}
