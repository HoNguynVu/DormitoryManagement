using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using BusinessObject.Entities;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(DormitoryDbContext context) : base(context)
        {
        }
        public async Task<Payment?> GetPaymentByReceiptIdAsync(string receiptId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.ReceiptID == receiptId);
        }
    }
}
