using DataAccess.Interfaces;
using System.Transactions;

namespace API.UnitOfWorks
{
    public interface IAuthUow : ITransactionManager
    {
        IAccountRepository Accounts { get; }
        IRefreshTokenRepository RefreshTokens { get; }
    }
}
