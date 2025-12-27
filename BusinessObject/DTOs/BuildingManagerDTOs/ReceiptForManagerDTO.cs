using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.BuildingManagerDTOs
{
    public class ReceiptForManagerDTO
    {
        public string ReceiptId { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string RoomName { get; set; }
        public string PaymentType { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }
}
