using BusinessObject.Entities;
using BusinessObject.DTOs.RoomDTOs;

namespace API.Services.Interfaces
{
    public interface IRoomService
    {
        Task<(bool Success, string Message, int StatusCode, IEnumerable<RegisRoomDTOs>)> GetRoomForRegistration(RoomFilterDto filter);

        Task<(bool Success, string Message, int StatusCode, RoomResponseDto?)> CreateRoomAsync(CreateRoomRequest request);

        Task<(bool Success, string Message, int StatusCode)> UpdateRoomAsync(UpdateRoomDto request);

        Task<(bool Success, string Message, int StatusCode)> DeleteRoomAsync(string roomId);

        Task<(bool Success, string Message, int StatusCode, RoomStatusDto?)> GetRoomStatusAsync(string roomId);

        Task<(bool Success, string Message, int StatusCode, IEnumerable<AvailableRoomDto>)> GetAvailableRoomsAsync(RoomFilterDto filter);
        Task<(bool Success, string Message, int StatusCode, IEnumerable<RoomResponseDto>)> GetAllRoomsForManagerAsync(string accountId);
    }
}
