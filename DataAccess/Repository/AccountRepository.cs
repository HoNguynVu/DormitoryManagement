using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly DormitoryDbContext _context;
        public AccountRepository(DormitoryDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Account>> GetAllAccounts()
        {
            return await _context.Accounts.ToListAsync();
        }
        public async Task<Account?> GetAccountById(string accountId)
        {
            return await _context.Accounts.FindAsync(accountId);
        }
        public async Task<Account?> GetAccountByUsername(string username)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == username);
        }
        public async Task<Account?> GetAccountByOtp(string otp)
        {
            return await _context.OtpCodes
                .Where(o => o.Code == otp)
                .Select(o => o.Account)
                .FirstOrDefaultAsync();
        }
        public void AddAccount(Account account)
        {
            _context.Accounts.Add(account);
        }
        public void UpdateAccount(Account account)
        {
            _context.Accounts.Update(account);
        }
        public void DeleteAccount(Account account)
        {
            _context.Accounts.Remove(account);
        }
    }
}
