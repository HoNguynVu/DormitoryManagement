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
    public class RelativeRepository : GenericRepository<Relative>, IRelativeRepository
    {
        public RelativeRepository(DormitoryDbContext context) : base(context)
        {
        }
    }
}
