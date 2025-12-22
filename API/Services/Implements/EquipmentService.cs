using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.EquipmentDTOs;

namespace API.Services.Implements
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IEquipmentUow _equipmentUow;
        public EquipmentService(IEquipmentUow equipmentUow)
        {
            _equipmentUow = equipmentUow;
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<SummaryEquipmentDto>? result)> GetAllEquipmentByRoomIdAsync(string roomId)
        {
            if (string.IsNullOrEmpty(roomId))
            {
                return (false, "Room ID cannot be null or empty.", 400, null);
            }
            var roomExists = await _equipmentUow.Rooms.GetByIdAsync(roomId);
            if (roomExists == null)
            {
                return (false, "Room ID does not exist.", 404, null);
            }
            try
            {
                var equipments = await _equipmentUow.Equipments.GetEquipmentsByRoomIdAsync(roomId);
                if (equipments == null || !equipments.Any())
                {
                    return (false, "No equipment found for the specified room ID.", 404, null);
                }
                var result = equipments.Select(e => new SummaryEquipmentDto
                {
                    EquipmentId = e.EquipmentID,
                    EquipmentName = e.EquipmentName
                }).ToList();
                return (true, "Equipments retrieved successfully.", 200, result);
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return (false, $"An error occurred: {ex.Message}", 500, null);
            }
        }
    }
}
