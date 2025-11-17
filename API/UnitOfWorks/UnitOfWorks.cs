using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore.Storage;
using DataAccess.Repository;

namespace API.UnitOfWorks
{
    public class UnitOfWork : IAuthUow
    {
        private readonly DormitoryDbContext _context;
        private IDbContextTransaction? _transaction;

        public IAccountRepository Accounts { get; }
        public IRefreshTokenRepository RefreshTokens { get; }


        public UnitOfWork(DormitoryDbContext context, IDbContextTransaction? dbContextTransaction)
        {
            _context = context;
            _transaction = dbContextTransaction;

            
            Accounts = new AccountRepository(_context);
            
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
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                if (_transaction != null)
                    await _transaction.RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                    await _transaction.DisposeAsync();
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                // 1. Chỉ Hủy nếu giao dịch đang tồn tại
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
            }
            finally
            {
                // 2. Luôn luôn dọn dẹp và giải phóng tài nguyên
                // (Kể cả khi RollbackAsync() thất bại)
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }
    }
}
