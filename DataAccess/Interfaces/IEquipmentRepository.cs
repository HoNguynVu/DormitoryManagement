using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IEquipmentRepository
    {
        Task<Equipment?> GetEquipmentByIdAsync(string equipmentId);
        void UpdateEquipment(Equipment equipment);
        void AddEquipment(Equipment equipment);
    }
}
