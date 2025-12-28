using BusinessObject.DTOs.ContractDTOs;
using BusinessObject.Entities;  
namespace API.Services.Interfaces
{
    public interface IContractService
    {
        Task<(bool Success, string Message, int StatusCode,string? receiptId)> RequestRenewalAsync(string studentId, int monthsToExtend);
        Task<(bool Success, string Message, int StatusCode, PendingRequestDto? dto)> GetPendingRenewalRequestAsync(string studentId);
        Task<(bool Success, string Message, int StatusCode, ContractDto? Data)> GetCurrentContractAsync(string studentId);
        Task<(bool Success, string Message, int StatusCode)> TerminateContractNowAsync(string studentId);
        //Manager
        Task<(bool Success, string Message, int StatusCode, IEnumerable<SummaryContractDto> dto)> GetContractFiltered(string? keyword, string? buildingName,string? status);
        Task<(bool Success, string Message, int StatusCode, Dictionary<string, int> stat)> GetOverviewContract();
        Task<(bool Success, string Message, int StatusCode)> ConfirmContractExtensionAsync(string contractId, int monthsAdded);
        Task<(bool Success, string Message, int StatusCode)> RejectRenewalAsync(RejectRenewalDto dto);
        Task<(bool Success, string Message, int StatusCode, DetailContractDto dto)> GetDetailContract(string contractId);
        Task<(bool Success, string Message, int StatusCode, IEnumerable<ExpiringContractDTO>)> GetExpiringContractByManager(int daysUntilExpiration, string managerId);
        Task<(bool Success, string Message, int StatusCode, int numContracts)> CountExpiringContractsByManager(int daysUntilExpiration, string managerID);
        Task<(bool Success, string Message, int StatusCode, string? ReceiptId, string? Type)> ChangeRoomAsync(ChangeRoomRequestDto request);
        //Student
        Task<(bool Success, string Message, int StatusCode, ContractDetailByStudentDto? dto)> GetContractDetailByStudentAsync(string accountId);
        Task<(bool Success, string Message)> RemindBulkExpiringAsync();
        Task<(bool Success, string Message)> RemindSingleStudentAsync(string studentId);
    }
}
