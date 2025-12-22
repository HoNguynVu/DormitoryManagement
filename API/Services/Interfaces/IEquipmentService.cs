using BusinessObject.DTOs.EquipmentDTOs;

namespace API.Services.Interfaces
{
    public interface IEquipmentService
    {
        Task<(bool Success,string Message,int StatusCode, IEnumerable<SummaryEquipmentDto>? result)> GetAllEquipmentByRoomIdAsync(string roomId);
    }
}
