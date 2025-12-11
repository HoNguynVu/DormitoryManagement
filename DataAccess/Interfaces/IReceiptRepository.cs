using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IReceiptRepository
    {
        void AddReceipt(Receipt receipt);
        Task<Receipt?> GetReceiptById(string receiptId);
        void UpdateReceipt(Receipt receipt);
    }
}
