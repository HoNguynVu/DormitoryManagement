using BusinessObject.DTOs.RegisDTOs;
using BusinessObject.Entities;
namespace API.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<(bool Success, string Message, int StatusCode)> CreateRegistrationForm(RegistrationFormRequest registrationForm);
        Task<(bool Success, string Message, int StatusCode)> UpdateStatusForm(UpdateFormRequest updateFormRequest);
    }
}
