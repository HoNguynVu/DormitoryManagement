using BusinessObject.DTOs.RoomTypeDTOs;
using BusinessObject.Entities;

namespace API.Services.Interfaces
{
    public interface IRoomTypeService
    {
        Task<(bool Success, string Message, int StatusCode, IEnumerable<GetRoomTypeDTO>)> GetAllRoomTypesAsync();
    }
}
