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
                    TypeName = rt.TypeName,
                    Description = rt.Description,
                    Capacity = rt.Capacity,
                    Price = rt.Price
                });
                return (true, "Room types retrieved successfully.", 200, roomTypes);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while retrieving room types: {ex.Message}", 500, Enumerable.Empty<GetRoomTypeDTO>());

            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> UpdateRoomTypeAsync(UpdateRoomTypeDTO updateRoomTypeDTO)
        {
            await _roomTypeUow.BeginTransactionAsync();
            try
            {
                var existingType = await _roomTypeUow.RoomTypes.GetByIdAsync(updateRoomTypeDTO.TypeID);
                if (existingType == null)
                {
                    return (false, "Room type not found.", 404);
                }
                var rooms = await _roomTypeUow.Rooms.GetRoomsByTypeIdAsync(updateRoomTypeDTO.TypeID);
                foreach (var room in rooms)
                {
                    if (room.CurrentOccupancy > updateRoomTypeDTO.Capacity)
                    {
                        return (false, $"Cannot update room type. Room {room.RoomName} has current occupancy {room.CurrentOccupancy} which exceeds the new capacity {updateRoomTypeDTO.Capacity}.", 400);
                    }
                    room.Capacity = updateRoomTypeDTO.Capacity;
                    _roomTypeUow.Rooms.Update(room);
                }
                existingType.TypeName = updateRoomTypeDTO.TypeName;
                existingType.Description = updateRoomTypeDTO.Description;
                existingType.Capacity = updateRoomTypeDTO.Capacity;
                existingType.Price = updateRoomTypeDTO.Price;
                _roomTypeUow.RoomTypes.Update(existingType);
                await _roomTypeUow.CommitAsync();
                return (true, "Room type updated successfully.", 200);
            }
            catch (Exception ex)
            {
                await _roomTypeUow.RollbackAsync();
                return (false, $"An error occurred while updating the room type: {ex.Message}", 500);
            }
        }
    }
}
