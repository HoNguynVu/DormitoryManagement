using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.RoomTypeDTOs;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class RoomTypeService : IRoomTypeService
    {
        private readonly IRoomTypeUow _roomTypeUow;
        public RoomTypeService(IRoomTypeUow roomTypeUow)
        {
            _roomTypeUow = roomTypeUow;
        }
        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<GetRoomTypeDTO>)> GetAllRoomTypesAsync()
        {
            try
            {
                var allTypes = await _roomTypeUow.RoomTypes.GetAllAsync();
                var roomTypes = allTypes.Select(rt => new GetRoomTypeDTO
                {
                    RoomTypeID = rt.RoomTypeID,
                    TypeName = rt.TypeName
                });
                return (true, "Room types retrieved successfully.", 200, roomTypes);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while retrieving room types: {ex.Message}", 500, Enumerable.Empty<GetRoomTypeDTO>());

            }
        }
    }
}
