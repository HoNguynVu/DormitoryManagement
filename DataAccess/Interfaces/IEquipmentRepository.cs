using BusinessObject.Entities;
using DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IEquipmentRepository : IGenericRepository<Equipment>
    {

        Task<IEnumerable<Equipment>> GetEquipmentsByRoomIdAsync(string roomId);
    }
}
