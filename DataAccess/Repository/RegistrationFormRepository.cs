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
    }
}
