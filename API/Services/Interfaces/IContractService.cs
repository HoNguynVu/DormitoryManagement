using BusinessObject.DTOs.ContractDTOs;
using BusinessObject.Entities;  
namespace API.Services.Interfaces
{
    public interface IContractService
    {
        Task<(bool Success, string Message, int StatusCode)> RequestRenewalAsync(string studentId, int monthsToExtend);
        Task<(bool Success, string Message, int StatusCode, Contract? Data)> GetCurrentContractAsync(string studentId);
        Task<(bool Success, string Message, int StatusCode)> TerminateContractNowAsync(string studentId);
        //Manager
        Task<(bool Success, string Message, int StatusCode, IEnumerable<SummaryContractDto>)> GetContractFiltered(string? keyword, string? buildingName,string? status);

        Task<(bool Success, string Message, int StatusCode)> ConfirmContractExtensionAsync(string contractId, int monthsAdded);
        Task<(bool Success, string Message, int StatusCode)> RejectRenewalAsync(RejectRenewalDto dto);
        Task<(bool Success, string Message, int StatusCode, IEnumerable<ExpiringContractDTO>)> GetExpiringContractByManager(int daysUntilExpiration, string managerId);
        Task<(bool Success, string Message, int StatusCode, int numContracts)> CountExpiringContractsByManager(int daysUntilExpiration, string managerID);
    }
}
