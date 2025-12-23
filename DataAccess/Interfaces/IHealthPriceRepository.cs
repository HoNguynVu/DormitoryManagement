using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IHealthPriceRepository : IGenericRepository<HealthInsurancePrice>
    {
        Task<HealthInsurancePrice?> GetHealthInsuranceByYear(int year);
    }
}
