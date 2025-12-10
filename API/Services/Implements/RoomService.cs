using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.RoomDTOs;

namespace API.Services.Implements
{
    public class RoomService : IRoomService
    {
        private readonly IRoomUow _roomUow;
        public RoomService(IRoomUow roomUow)
        {
            _roomUow = roomUow;
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<RegisRoomDTOs>)> GetRoomForRegistration()
        {
            try
            {
                var rooms = await _roomUow.Rooms.GetAllRooms();
                var regisRoomDTOs = new List<RegisRoomDTOs>();
                foreach (var room in rooms)
                {
                    var type = await _roomUow.RoomTypes.GetRoomTypeById(room.RoomTypeId);
                    int registeredOccupancy = await _roomUow.RegistrationForms.CountRegistrationFormsByRoomId(room.RoomId);
                    regisRoomDTOs.Add(new RegisRoomDTOs
                    {
                        RoomId = room.RoomId,
                        RoomName = room.RoomName,
                        RoomType = type.TypeName ?? "Standard",
                        Price = type.Price,
                        Capacity = room.Capacity,
                        CurrentOccupancy = room.CurrentOccupancy,
                        RegisteredOccupancy = registeredOccupancy
                    });
                }
                return (true, "Rooms retrieved successfully", 200, regisRoomDTOs);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", 500, Enumerable.Empty<RegisRoomDTOs>());
            }
        }
    }
}
