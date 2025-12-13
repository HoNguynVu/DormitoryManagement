using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IRoomRepository : IGenericRepository<Room>
    {
        Task<IEnumerable<Room>> GetAllRoomsWithTypesAsync();
        Task<IEnumerable<Room>> FindBySpecificationAsync(Expression<Func<Room, bool>> spec);
    }
}
