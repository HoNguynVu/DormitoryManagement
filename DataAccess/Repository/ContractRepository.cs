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
    public class ContractRepository : IContractRepository
    {
        private readonly DormitoryDbContext _context;
        public ContractRepository(DormitoryDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Contract>> GetAllContracts()
        {
            return await _context.Contracts.ToListAsync();
        }
        public async Task<Contract?> GetContractById(string contractId)
        {
            return await _context.Contracts.FindAsync(contractId);
        }
        public async Task<IEnumerable<Contract>> GetContractsByStudentId(string studentId)
        {
            return await _context.Contracts
                .Where(c => c.StudentId == studentId)
                .ToListAsync();
        }
        public void AddContract(Contract contract)
        {
            _context.Contracts.Add(contract);
        }
        public void UpdateContract(Contract contract)
        {
            _context.Contracts.Update(contract);
        }
        public void DeleteContract(Contract contract)
        {
            _context.Contracts.Remove(contract);
        }

        public async Task<int> CountContractsByRoomIdAndStatus(string roomId, string status)
        {
            return await _context.Contracts
                .Where(c => c.RoomId == roomId && c.ContractStatus == status)
                .CountAsync();
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
    }
}
