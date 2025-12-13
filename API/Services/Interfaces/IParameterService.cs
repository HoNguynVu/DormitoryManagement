using BusinessObject.DTOs.ParameterDTOs;
using BusinessObject.Entities;

namespace API.Services.Interfaces
{
    public interface IParameterService
    {
        Task<(bool Success, string Message, int StatusCode)> SetNewParameter(CreateParameterDTO parameter);
        Task<(bool Success, string Message, int StatusCode, IEnumerable<Parameter> listPara)> GetAllParameter();
    }
}
