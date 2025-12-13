using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using BusinessObject.Entities;
using DataAccess.Models;

namespace DataAccess.Repository
{
    public class ParameterRepository : GenericRepository<Parameter>, IParameterRepository
    {
        public ParameterRepository(DormitoryDbContext context) : base(context)
        {
        }
        public async Task<Parameter?> GetActiveParameterAsync()
        {
            return await Task.FromResult(_dbSet.FirstOrDefault(p => p.IsActive));
        }
    }
}
