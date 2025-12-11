using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IPaymentRepository
    {
        void AddPayment(Payment payment);
        Task<Payment?> GetPaymentById(int paymentId);
        void UpdatePayment(Payment payment);
    }
}
