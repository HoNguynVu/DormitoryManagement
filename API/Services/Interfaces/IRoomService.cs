using BusinessObject.Entities;
using BusinessObject.DTOs.RoomDTOs;

namespace API.Services.Interfaces
{
    public interface IRoomService
    {
        Task<(bool Success, string Message, int StatusCode, IEnumerable<RegisRoomDTOs>)> GetRoomForRegistration();

        Task<(bool Success, string Message, int StatusCode, RoomResponseDto?)> CreateRoomAsync(CreateRoomRequest request);

        Task<(bool Success, string Message, int StatusCode)> UpdateRoomAsync(UpdateRoomDto request);

        Task<(bool Success, string Message, int StatusCode)> DeleteRoomAsync(string roomId);
    }
}
