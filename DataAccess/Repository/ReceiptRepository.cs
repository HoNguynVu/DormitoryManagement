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
    public class ReceiptRepository : IReceiptRepository
    {
        private readonly DormitoryDbContext _context;
        public ReceiptRepository(DormitoryDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Receipt>> GetAllReceipts()
        {
            return await _context.Receipts.ToListAsync();
        }
        public async Task<Receipt?> GetReceiptById(string receiptId)
        {
            return await _context.Receipts.FindAsync(receiptId);
        }
        public void AddReceipt(Receipt receipt)
        {
            _context.Receipts.Add(receipt);
        }
        public void UpdateReceipt(Receipt receipt)
        {
            _context.Receipts.Update(receipt);
        }
    }
}
