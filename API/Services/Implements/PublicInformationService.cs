using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class PublicInformationService : IPublicInformationService
    {
        private readonly IPublicInformationUow publicInformationUow;
        public PublicInformationService(IPublicInformationUow publicInformationUow)
        {
            this.publicInformationUow = publicInformationUow;
        }
        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<School> Schools)> GetSchoolsAsync()
        {
            try
            {
                var schools = await publicInformationUow.Schools.GetAllAsync();
                return (true, "Schools retrieved successfully.", 200, schools);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while retrieving schools: {ex.Message}", 500, Enumerable.Empty<School>());
            }
        }
        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<Priority> Priorities)> GetPrioritiesAsync()
        {
            try
            {
                var priorities = await publicInformationUow.Priorities.GetAllAsync();
                return (true, "Priorities retrieved successfully.", 200, priorities);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while retrieving priorities: {ex.Message}", 500, Enumerable.Empty<Priority>());
            }
        }
    }
}
