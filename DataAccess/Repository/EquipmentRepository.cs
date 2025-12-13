using DataAccess.Interfaces;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly DormitoryDbContext _context;
        public EquipmentRepository(DormitoryDbContext context)
        {
            _context = context;
        }

        public async Task<BusinessObject.Entities.Equipment?> GetEquipmentByIdAsync(string equipmentId)
        {
            return await _context.Equipment.FindAsync(equipmentId);
        }

        public void UpdateEquipment(BusinessObject.Entities.Equipment equipment)
        {
            _context.Equipment.Update(equipment);
        }
        public void AddEquipment(BusinessObject.Entities.Equipment equipment)
        {
            _context.Equipment.Add(equipment);
        }
    }
}
