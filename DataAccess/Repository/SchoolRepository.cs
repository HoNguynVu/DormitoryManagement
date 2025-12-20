using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;

namespace DataAccess.Repository
{
    public class SchoolRepository : GenericRepository<School>, ISchoolRepository
    {
        public SchoolRepository(DormitoryDbContext context) : base(context)
        {
        }
    }
}
