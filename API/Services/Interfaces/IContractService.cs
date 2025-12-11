namespace API.Services.Interfaces
{
    public interface IContractService
    {
        Task<(bool Success, string Message, int StatusCode)> RequestRenewalAsync(string studentId, int monthsToExtend);

        Task<(bool Success, string Message, int StatusCode)> ConfirmContractExtensionAsync(string contractId, int monthsAdded);
    }
}
