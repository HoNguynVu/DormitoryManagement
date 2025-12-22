using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IMaintenanceRepository
    {
        void Add(MaintenanceRequest request);

        void Update(MaintenanceRequest request);

        Task<IEnumerable<MaintenanceRequest>> GetMaintenanceByStudentIdAsync(string studentId);
        Task<MaintenanceRequest?> GetMaintenanceByIdAsync(string maintenanceId);
        Task<IEnumerable<MaintenanceRequest>> GetMaintenanceFilteredAsync(string? keyword, string? status, string? equipmentName);

        Task<MaintenanceRequest?> GetMaintenanceDetailAsync(string maintenanceId);
    }
}
