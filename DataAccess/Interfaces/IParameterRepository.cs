using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IParameterRepository : IGenericRepository<Parameter>
    {
        Task<Parameter?> GetActiveParameterAsync();
    }
}
