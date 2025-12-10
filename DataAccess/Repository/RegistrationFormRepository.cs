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
    public class RegistrationFormRepository : IRegistrationFormRepository
    {
        private readonly DormitoryDbContext _context;
        public RegistrationFormRepository(DormitoryDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<RegistrationForm>> GetAllForms()
        {
            return await _context.RegistrationForms.ToListAsync();
        }
        public async Task<RegistrationForm?> GetByIdAsync(string formId)
        {
            return await _context.RegistrationForms.FindAsync(formId);
        }
        public void Add(RegistrationForm registrationForm)
        {
            _context.RegistrationForms.Add(registrationForm);
        }
        public void Update(RegistrationForm registrationForm)
        {
            _context.RegistrationForms.Update(registrationForm);
        }
        public void Delete(RegistrationForm registrationForm)
        {
            _context.RegistrationForms.Remove(registrationForm);
        }
        public async Task<int> CountRegistrationFormsByRoomId(string roomId)
        {
            var threshold = DateTime.UtcNow.AddMinutes(-10);

            return await _context.RegistrationForms
                .CountAsync(f => f.RoomId == roomId
                                 && f.Status == "Pending"
                                 && f.RegistrationTime >= threshold);
        }
        public async Task<Dictionary<string, int>> CountPendingFormsByRoomAsync()
        {
            // Chỉ đếm các đơn Pending còn hạn (ví dụ trong 10-15p gần nhất)
            // Nếu không cần lọc thời gian thì bỏ dòng RegistrationTime
            var threshold = DateTime.UtcNow.AddMinutes(-15);

            var query = await _context.RegistrationForms
                .Where(f => f.Status == "Pending" && f.RegistrationTime >= threshold)
                .GroupBy(f => f.RoomId)
                .Select(g => new { RoomId = g.Key, Count = g.Count() })
                .ToListAsync();

            // Chuyển về Dictionary để tra cứu cực nhanh: Key = RoomId, Value = Count
            return query.ToDictionary(x => x.RoomId, x => x.Count);
        }
    }
}
