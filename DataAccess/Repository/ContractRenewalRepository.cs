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
    public class ContractRenewalRepository : IContractRenewalRepository
    {
        private readonly DormitoryDbContext _context;
        public ContractRenewalRepository(DormitoryDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountViolationsByStudentAsync(string studentId)
        {
            return await _context.Violations
                            .CountAsync(v => v.StudentId == studentId);
        }

        // Lấy hợp đồng Active
        public async Task<Contract?> GetActiveContractByStudentIdAsync(string studentId)
        {
            return await _context.Contracts
                .Include(c => c.Student)
                .Include(c => c.Room)
                .Where(c => c.StudentId == studentId && c.ContractStatus == "Active") 
                .OrderByDescending(h => h.EndDate)
                .FirstOrDefaultAsync();
        }

        // Lấy hợp đồng theo ID
        public async Task<Contract?> GetContractByIdAsync(string contractId)
        {
            return await _context.Contracts
                .Include(c => c.Room) 
                .FirstOrDefaultAsync(h => h.ContractId == contractId);
        }

        public async Task<bool> HasPendingRenewalRequestAsync(string studentId)
        {
            return await _context.Receipts
                 .AnyAsync(i => i.StudentId == studentId
                                && i.PaymentType == "Renewal"
                                && i.Status == "Pending");
        }

        // Update Contract
        public void UpdateContract(Contract contract)
        {
            _context.Contracts.Update(contract);
        }
        public void AddRenewalReceipt(Receipt receipt)
        {
            _context.Receipts.AddAsync(receipt);
        }
    }
}
