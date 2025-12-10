using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IContractRenewalRepository
    {
        // 1. Lấy hợp đồng đang có hiệu lực của sinh viên
        Task<Contract?> GetActiveContractByStudentIdAsync(string studentId);

        // 2. Lấy chi tiết hợp đồng theo ID
        Task<Contract?> GetContractByIdAsync(string contractId);

        // 3. Kiểm tra lịch sử vi phạm ( số lượng vi phạm )
        Task<int> CountViolationsByStudentAsync(string studentId);

        // 4. Cập nhật hợp đồng 
        void UpdateContract(Contract contract);

        // 5. Kiểm tra xem sinh viên đã có yêu cầu gia hạn (Invoice pending) nào chưa.
        Task<bool> HasPendingRenewalRequestAsync(string studentId);

        void AddRenewalReceipt(Receipt receipt);

    }
}
