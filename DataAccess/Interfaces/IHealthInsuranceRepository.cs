using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IHealthInsuranceRepository
    {
        // Lấy BHYT đang hiệu lực của sinh viên (để check trùng hoặc gia hạn)
        Task<HealthInsurance?> GetActiveInsuranceByStudentIdAsync(string studentId);

        // Check xem có yêu cầu mua BHYT nào đang chờ thanh toán không
        Task<bool> HasPendingInsuranceRequestAsync(string studentId);

        // Hàm lấy BTHY mới nhất 
        Task<HealthInsurance?> GetLatestInsuranceByStudentIdAsync(string studentId);
        // Thêm mới
        void Add(HealthInsurance insurance);

        // Cập nhật
        void Update(HealthInsurance insurance);

    }
}
