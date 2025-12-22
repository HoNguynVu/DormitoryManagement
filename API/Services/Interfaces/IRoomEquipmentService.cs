namespace API.Services.Interfaces
{
    public interface IRoomEquipmentService
    {
        Task<(bool Success,string Message,int StatusCode)> ChangeStatusAsync(string roomId, string equipmentId, int quantity, string fromStatus, string toStatus);
    }
}
