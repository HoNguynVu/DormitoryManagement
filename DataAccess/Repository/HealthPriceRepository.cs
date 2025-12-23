using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class HealthPriceRepository : GenericRepository<HealthInsurancePrice>, IHealthPriceRepository
    {
        public HealthPriceRepository(DormitoryDbContext context) : base(context) { }

        public async Task<HealthInsurancePrice?> GetHealthInsuranceByYear(int year)
        {
            return await _dbSet
                .Where(hp => hp.IsActive == true && hp.Year == year)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
