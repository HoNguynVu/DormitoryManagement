namespace API.Services.Interfaces
{
    public interface IContractService
    {
        Task<string> RequestRenewalAsync(string studentId, int monthsToExtend);

        Task ConfirmContractExtensionAsync(string contractId, int monthsAdded);
    }
}
