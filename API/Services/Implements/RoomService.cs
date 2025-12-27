using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.RoomDTOs;
using BusinessObject.Entities;
using BusinessObject.Factories;
using DataAccess.Specifications;

namespace API.Services.Implements
{
    public class RoomService : IRoomService
    {
        private readonly IRoomUow _roomUow;
        public RoomService(IRoomUow roomUow)
        {
            _roomUow = roomUow;
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<RegisRoomDTOs>)> GetRoomForRegistration(RoomFilterDto filter)
        {
            try
            {
                var spec = RoomSpecifications.ByFilter(filter);
                var rooms = await _roomUow.Rooms.FindBySpecificationAsync(spec);
                var pendingCountsDict = await _roomUow.RegistrationForms.CountPendingFormsByRoomAsync();
                var regisRoomDTOs = new List<RegisRoomDTOs>();

                foreach (var room in rooms)
                {
                    int pendingCount = pendingCountsDict.GetValueOrDefault(room.RoomID, 0);
                    if (room.Capacity == room.CurrentOccupancy)
                    {
                        continue; 
                    }
                    regisRoomDTOs.Add(new RegisRoomDTOs
                    {
                        RoomId = room.RoomID,
                        RoomName = room.RoomName,
                        RoomType = room.RoomType?.TypeName ?? "Unknown",
                        BuildingName = room.Building?.BuildingName ?? "Unknown",
                        Price = room.RoomType?.Price ?? 0,
                        Capacity = room.Capacity,
                        CurrentOccupancy = room.CurrentOccupancy,
                        RegisteredOccupancy = pendingCount, 
                        Gender = room.Gender
                    });
                }

                return (true, "Rooms retrieved successfully", 200, regisRoomDTOs);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", 500, Enumerable.Empty<RegisRoomDTOs>());
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, RoomResponseDto?)> CreateRoomAsync(CreateRoomRequest request)
        {
            if (request is null) return (false, "Request is null", 400, null);

            if (!await _roomUow.BuildingExistsAsync(request.BuildingId))
                return (false, "Building not found", 400, null);

            var roomType = await _roomUow.RoomTypes.GetByIdAsync(request.RoomTypeId);
            if (roomType == null)
                return (false, "RoomType not found", 400, null);

            var room = RoomFactory.Create(request, roomType);
            var existing = await _roomUow.Rooms.GetByIdAsync(room.RoomID);
            if (existing != null)
                return (false, "Room already exists", 409, null);

            try
            {
                _roomUow.Rooms.Add(room);
                await _roomUow.SaveChangesAsync();

                var response = new RoomResponseDto
                {
                    RoomID = room.RoomID,
                    RoomName = room.RoomName,
                    BuildingID = room.BuildingID,
                    RoomTypeID = room.RoomTypeID,
                    Capacity = room.Capacity,
                    CurrentOccupancy = room.CurrentOccupancy,
                    RoomStatus = room.RoomStatus,
                    IsUnderMaintenance = room.IsUnderMaintenance,
                    IsBeingCleaned = room.IsBeingCleaned
                };

                return (true, "Room created", 201, response);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to create room: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> UpdateRoomAsync(UpdateRoomDto request)
        {
            if (request is null) return (false, "Request is null", 400);
            await _roomUow.BeginTransactionAsync();
            try
            {
                var room = await _roomUow.Rooms.GetByIdAsync(request.RoomID);
                if (room is null)
                {
                    await _roomUow.RollbackAsync();
                    return (false, "Room not found", 404);
                }    
                room.IsUnderMaintenance = request.IsUnderMaintenance ?? room.IsUnderMaintenance;
                room.IsBeingCleaned = request.IsBeingCleaned ?? room.IsBeingCleaned;
                room.ChangeMaintenanceStatus(room.IsUnderMaintenance);
                room.ChangeCleaningStatus(room.IsBeingCleaned);
                room.Gender = request.Gender;
                _roomUow.Rooms.Update(room);
                await _roomUow.CommitAsync();
                return (true, "Room updated", 200);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to update room: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> DeleteRoomAsync(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId)) return (false, "Invalid room id", 400);

            try
            {
                var existing = await _roomUow.Rooms.GetByIdAsync(roomId);
                if (existing is null) return (false, "Room not found", 404);

                _roomUow.Rooms.Delete(existing);
                await _roomUow.SaveChangesAsync();

                return (true, "Room deleted", 200);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to delete room: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, RoomStatusDto?)> GetRoomStatusAsync(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId)) return (false, "Room id is required", 400, null);

            var room = await _roomUow.Rooms.GetByIdAsync(roomId);
            if (room == null) return (false, "Room not found", 404, null);

            // Count active contracts and pending registration forms
            var activeCount = await _roomUow.Contracts.CountContractsByRoomIdAndStatus(roomId, "Active");
            var pendingCount = await _roomUow.RegistrationForms.CountRegistrationFormsByRoomId(roomId);

            var occupied = Math.Max(room.CurrentOccupancy, activeCount); // prefer DB occupancy or contracts
            var available = Math.Max(0, room.Capacity - (occupied + pendingCount));

            var dto = new RoomStatusDto
            {
                RoomID = room.RoomID,
                Capacity = room.Capacity,
                Occupied = occupied,
                AvailableBeds = available,
                RoomStatus = room.RoomStatus
            };

            return (true, "Success", 200, dto);
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<AvailableRoomDto>)> GetAvailableRoomsAsync(RoomFilterDto filter)
        {
            try
            {
                var spec = RoomSpecifications.ByFilter(filter);

                var rooms = await _roomUow.Rooms.FindBySpecificationAsync(spec);

                var pendingDict = await _roomUow.RegistrationForms.CountPendingFormsByRoomAsync();

                var result = new List<AvailableRoomDto>();
                foreach (var room in rooms)
                {
                    var activeCount = await _roomUow.Contracts.CountContractsByRoomIdAndStatus(room.RoomID, "Active");
                    var pending = pendingDict.GetValueOrDefault(room.RoomID, 0);
                    var occupied = Math.Max(room.CurrentOccupancy, activeCount);
                    var availableBeds = Math.Max(0, room.Capacity - (occupied + pending));

                    if (filter?.OnlyAvailable == true && availableBeds <= 0)
                        continue;

                    result.Add(new AvailableRoomDto
                    {
                        RoomID = room.RoomID,
                        RoomName = room.RoomName,
                        Capacity = room.Capacity,
                        Occupied = occupied,
                        AvailableBeds = availableBeds,
                        Price = room.RoomType?.Price ?? 0,
                        RoomType = room.RoomType?.TypeName ?? string.Empty
                    });
                }

                return (true, "Success", 200, result);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", 500, Enumerable.Empty<AvailableRoomDto>());
            }
        }
        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<RoomResponseDto>)> GetAllRoomsForManagerAsync(string accountId)
        {
            try
            {
                var manager = await _roomUow.BuildingManagers.GetByAccountIdAsync(accountId);
                if (manager == null)
                    return (false, "Building manager not found", 404, Enumerable.Empty<RoomResponseDto>());
                var rooms = await _roomUow.Rooms.GetRoomByManagerIdAsync(manager.ManagerID);
                if (rooms == null || !rooms.Any())
                    return (false, "No rooms found for this manager", 404, Enumerable.Empty<RoomResponseDto>());
                var roomDtos = rooms.Select(room => new RoomResponseDto
                {
                    RoomID = room.RoomID,
                    RoomName = room.RoomName,
                    BuildingID = room.BuildingID,
                    BuildingName = room.Building?.BuildingName ?? "N/A",
                    RoomTypeID = room.RoomTypeID,
                    RoomTypeName = room.RoomType?.TypeName ?? "N/A",
                    Capacity = room.Capacity,
                    Gender = room.Gender,
                    CurrentOccupancy = room.CurrentOccupancy,
                    RoomStatus = room.RoomStatus,
                    IsUnderMaintenance = room.IsUnderMaintenance,
                    IsBeingCleaned = room.IsBeingCleaned
                });

                return (true, "Rooms retrieved successfully", 200, roomDtos);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", 500, Enumerable.Empty<RoomResponseDto>());
            }
        }
    }
}
