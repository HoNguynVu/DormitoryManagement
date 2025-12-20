using API.Services.Common;
using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ConfirmDTOs;
using BusinessObject.DTOs.RegisDTOs;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IRegistrationUow _registrationUow;
        private readonly IEmailService _emailService;
        private readonly ILogger<IRegistrationService> _logger;
        public RegistrationService(IRegistrationUow registrationUow, IEmailService emailService, ILogger<IRegistrationService> logger)
        {
            _registrationUow = registrationUow;
            _emailService = emailService;
            _logger = logger;
        }
        public async Task<(bool Success, string Message, int StatusCode, string? registrationId)> CreateRegistrationForm(RegistrationFormRequest registrationForm) 
        {
            var student = await _registrationUow.Students.GetStudentByAccountIdAsync(registrationForm.AccountId);
            if (student == null)
            {
                return (false, "Student not found.", 404, null);
            }
            var contract = await _registrationUow.Contracts.GetActiveContractByStudentId(student.StudentID);
            if (contract != null)
            {
                return (false, "Student already has an active contract.", 400, null);
            }
            await _registrationUow.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                int studentsInRoom = await _registrationUow.Contracts.CountContractsByRoomIdAndStatus(registrationForm.RoomId, "Active");
                int pendingForms = await _registrationUow.RegistrationForms.CountRegistrationFormsByRoomId(registrationForm.RoomId);
                int occupancy = studentsInRoom + pendingForms;
                var room = await _registrationUow.Rooms.GetByIdAsync(registrationForm.RoomId);
                if (room == null)
                {
                    await _registrationUow.RollbackAsync(); 
                    return (false, "Room not found.", 404, null);
                }
                
                if (room.Gender != student.Gender)
                {
                    await _registrationUow.RollbackAsync();
                    return (false, "Student's Gender is not suitable", 400, null);
                }
                int capacity = room.Capacity;

                if (occupancy >= capacity)
                {
                    await _registrationUow.RollbackAsync();
                    return (false, "Room is already full.", 409, null);
                }

                var form = new RegistrationForm
                {
                    FormID = "RF-" + IdGenerator.GenerateUniqueSuffix(),
                    StudentID = student.StudentID,
                    RoomID = registrationForm.RoomId,
                    RegistrationTime = DateTime.UtcNow,
                    Status = "Pending"
                };

                _registrationUow.RegistrationForms.Add(form);
                await _registrationUow.CommitAsync();
                return (true, "Registration form created successfully, you have 10 mins to confirms payment.", 201, form.FormID);
            }
            catch (Exception ex)
            {
                await _registrationUow.RollbackAsync();
                return (false, $"Failed to create registration form: {ex.Message}", 500, null);

            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> UpdateStatusForm(UpdateFormRequest updateFormRequest)
        {
            var form = await _registrationUow.RegistrationForms.GetByIdAsync(updateFormRequest.FormId);
            if (form == null)
            {
                return (false, "Registration form not found.", 404);
            }
            form.Status = updateFormRequest.Status;
            await _registrationUow.BeginTransactionAsync();
            try
            {
                _registrationUow.RegistrationForms.Update(form);
                await _registrationUow.CommitAsync();
                
            }
            catch (Exception ex)
            {
                await _registrationUow.RollbackAsync();
                return (false, $"Failed to update registration form status: {ex.Message}", 500);
            }
            //gởi mail yêu cầu thanh toán ở đây nếu status = approved
            return (true, "Registration form status updated successfully.", 200);
        }

        public async Task<(bool Success, string Message, int StatusCode)> ConfirmPaymentForRegistration(string registrationId)
        {
            var registration = await _registrationUow.RegistrationForms.GetByIdAsync(registrationId);
            if (registration == null)
            {
                return (false, "Registration form not found.", 404);
            }
            var newContract = new Contract
            {
                ContractID = "CT-" + IdGenerator.GenerateUniqueSuffix(),
                StudentID = registration.StudentID,
                RoomID = registration.RoomID,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ContractStatus = "Active",
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(6)
            };
            var room = await _registrationUow.Rooms.GetByIdAsync(registration.RoomID);
            if (room == null)
            {
                return (false, "Room not found.", 404);
            }
            await _registrationUow.BeginTransactionAsync();
            try
            {
                registration.Status = "Confirmed";
                room.CurrentOccupancy += 1;
                _registrationUow.Rooms.Update(room);        
                _registrationUow.RegistrationForms.Update(registration);
                _registrationUow.Contracts.Add(newContract);
                await _registrationUow.CommitAsync();

                // Gửi email xác nhận
                var student = await _registrationUow.Students.GetByIdAsync(registration.StudentID);
                if (student == null)
                    return (false, "Student not found.", 404);
                var receipt = await _registrationUow.Receipts.GetReceiptByTypeAndRelatedIdAsync(PaymentConstants.TypeRegis,newContract.ContractID);
                if (receipt == null)
                    return (false, "Receipt not found.", 404);
                decimal depositAmount = receipt != null ? receipt.Amount : 0;
                var emailDto = new DormRegistrationSuccessDto
                {
                    StudentEmail = student.Email,
                    StudentName = student.FullName,
                    ContractCode = newContract.ContractID,
                    BuildingName = newContract.Room.Building.BuildingName,
                    RoomName = newContract.Room.RoomName,
                    RoomType = newContract.Room?.RoomType?.TypeName ?? "",
                    StartDate = newContract.StartDate,
                    EndDate = newContract.EndDate.Value,
                    DepositAmount = depositAmount,
                    RoomFeePerMonth = newContract.Room.RoomType.Price
                };

                try
                {
                    await _emailService.SendRegistrationPaymentEmailAsync(emailDto);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send dorm registration success email to {Email}", student.Email); 
                }
                return (true, "Payment confirmed and contract created successfully.", 200);
            }
            catch (Exception ex)
            {
                await _registrationUow.RollbackAsync();
                return (false, $"Failed to confirm payment: {ex.Message}", 500);
            }
        }
    }
}
