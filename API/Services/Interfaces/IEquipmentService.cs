namespace API.Services.Interfaces
{
    public interface IEquipmentService
    {
        Task<(bool Success,string Message,int StatusCode,Dictionary<string,string>? result)> GetAllEquipmentByRoomIdAsync(string roomId);
    }
}
