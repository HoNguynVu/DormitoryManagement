using API.Services.Interfaces;
using BusinessObject.DTOs.ParameterDTOs;
using BusinessObject.Entities;
using API.UnitOfWorks;

namespace API.Services.Implements
{
    public class ParameterService : IParameterService
    {
        private readonly IParameterUow _parameterUow;
        public ParameterService(IParameterUow parameterUow)
        {
            _parameterUow = parameterUow;
        }
        public async Task<(bool Success, string Message, int StatusCode)> SetNewParameter(CreateParameterDTO parameter)
        {
            // Validate input
            if (parameter == null)
            {
                return (false, "Invalid parameter data", 400);
            }
            var activeParameter = await _parameterUow.Parameters.GetActiveParameterAsync();
            var newParameter = new Parameter
            {
                DefaultElectricityPrice = parameter.DefaultElectricityPrice,
                DefaultWaterPrice = parameter.DefaultWaterPrice,
                EffectiveDate = DateTime.UtcNow,
                IsActive = true
            };
            await _parameterUow.BeginTransactionAsync();
            try
            {
                // Deactivate current active parameter if exists
                if (activeParameter != null)
                {
                    activeParameter.IsActive = false;
                    _parameterUow.Parameters.Update(activeParameter);
                }
                _parameterUow.Parameters.Add(newParameter);
                // Add new parameter
                await _parameterUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _parameterUow.RollbackAsync();
                return (false, $"Failed to set new parameter: {ex.Message}", 500);
            }
            return (true, "New parameter set successfully", 200);
        }
        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<Parameter> listPara)> GetAllParameter()
        {
            try
            {
                var parameters = await _parameterUow.Parameters.GetAllAsync();
                if (parameters == null)
                {
                    return (false, "No parameters found", 404, new List<Parameter>());
                }
                return (true, "Parameters retrieved successfully", 200, parameters);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to retrieve parameters: {ex.Message}", 500, new List<Parameter>());
            }
        }
    }
}
