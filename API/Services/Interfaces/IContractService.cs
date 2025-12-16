using BusinessObject.Entities;  
namespace API.Services.Interfaces
{
    public interface IContractService
    {
        Task<(bool Success, string Message, int StatusCode)> RequestRenewalAsync(string studentId, int monthsToExtend);
        Task<(bool Success, string Message, int StatusCode, Contract? Data)> GetCurrentContractAsync(string studentId);
        Task<(bool Success, string Message, int StatusCode)> TerminateContractNowAsync(string studentId);

        Task<(bool Success, string Message, int StatusCode)> ConfirmContractExtensionAsync(string contractId, int monthsAdded);
        Task<(bool Success, string Message, int StatusCode, IEnumerable<Contract>)> GetExpiringContractByManager(int daysUntilExpiration, string managerId);
    }
}
