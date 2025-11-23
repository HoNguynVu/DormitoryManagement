using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> GetAllAccounts();
        Task<Account?> GetAccountById(string accountId);
        Task<Account?> GetAccountByUsername(string username);
        Task<Account?> GetAccountByOtp(string otp);
        void AddAccount(Account account);
        void UpdateAccount(Account account);
        void DeleteAccount(Account account);
    }
}
