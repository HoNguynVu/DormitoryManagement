using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IRegistrationFormRepository : IGenericRepository<RegistrationForm>
    {
        Task<int> CountRegistrationFormsByRoomId(string roomId);
        Task<Dictionary<string, int>> CountPendingFormsByRoomAsync();
    }
}
