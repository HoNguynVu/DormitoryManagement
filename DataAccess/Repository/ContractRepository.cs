using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class ContractRepository : IContractRepository
    {
        private readonly DormitoryDbContext _context;
        public ContractRepository(DormitoryDbContext context) 
        {
            _context = context;
        }
        public async Task<Contract?> GetActiveContractByStudentId(string studentId)
        {
            return await _context.Contracts
                .Include(c => c.Student)
                .Include(c => c.Room)
                .Where(c => c.StudentId == studentId && (c.ContractStatus == "Active" || c.ContractStatus == "Pending"))
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefaultAsync();
        }
        public async Task<Contract?> GetContractById(string contractId) 
        {
            return await _context.Contracts
                .Include(c => c.Student)
                .Include(c => c.Room)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);
        }
        public void UpdateContract (Contract contract)
        {
            _context.Contracts.Update(contract);
        }
    }
}
