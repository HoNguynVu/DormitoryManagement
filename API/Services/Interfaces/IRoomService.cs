using BusinessObject.Entities;
using BusinessObject.DTOs.RoomDTOs;

namespace API.Services.Interfaces
{
    public interface IRoomService
    {
        Task<(bool Success, string Message, int StatusCode, IEnumerable<RegisRoomDTOs>)> GetRoomForRegistration();
    }
}
