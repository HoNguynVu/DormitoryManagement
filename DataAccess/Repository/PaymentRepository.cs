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
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private readonly DormitoryDbContext _context;
        public PaymentRepository(DormitoryDbContext context) : base(context)
        {
        }
    }
}
