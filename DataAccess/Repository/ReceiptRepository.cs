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
    public class ReceiptRepository : GenericRepository<Receipt>, IReceiptRepository
    {
        public ReceiptRepository(DormitoryDbContext context) : base(context)
        {
        }
        public async Task<Receipt?> GetReceiptByTypeAndRelatedIdAsync(string paymentType, string relatedId)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.PaymentType == paymentType && r.RelatedObjectID == relatedId);
        }
    }
}
