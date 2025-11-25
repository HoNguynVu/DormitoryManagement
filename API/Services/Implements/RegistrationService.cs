using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.RegisDTOs;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IRegistrationUow _registrationUow;
        public RegistrationService(IRegistrationUow registrationUow)
        {
            _registrationUow = registrationUow;
        }
        public async Task<(bool Success, string Message, int StatusCode)> CreateRegistrationForm(RegistrationFormRequest registrationForm)
        {
            var contract = await _registrationUow.Contracts.GetActiveContractByStudentId(registrationForm.StudentId);
            if (contract != null)
            {
                return (false, "Student already has an active contract.", 400);
            }
            var form = new RegistrationForm
            {
                FormId = "RF-" + IdGenerator.GenerateUniqueSuffix(),
                StudentId = registrationForm.StudentId,
                RoomId = registrationForm.RoomId,
                RegistrationTime = DateTime.UtcNow,
                Status = "Pending"
            };
            await _registrationUow.BeginTransactionAsync();
            try
            {
                _registrationUow.RegistrationForms.Add(form);
                await _registrationUow.CommitAsync();
                return (true, "Registration form created successfully.", 201);
            }
            catch (Exception ex)
            {
                await _registrationUow.RollbackAsync();
                return (false, $"Failed to create registration form: {ex.Message}", 500);

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
    }
}
