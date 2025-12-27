using BusinessObject.DTOs.EquipmentDTO;

namespace API.Services.Interfaces
{
    public interface IRoomEquipmentService
    {
        Task<(bool Success, string Message, int StatusCode, IEnumerable<string>? list)> GetAllEquipment();
        Task<(bool Success, string Message, int StatusCode, IEnumerable<EquipmentOfRoomDTO> dto)> GetEquipmentsByRoomIdAsync(string roomId);
        Task<(bool Success,string Message,int StatusCode)> ChangeStatusAsync(string roomId, string equipmentId, int quantity, string fromStatus, string toStatus);
    }
}
