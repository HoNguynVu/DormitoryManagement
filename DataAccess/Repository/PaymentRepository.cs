using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using BusinessObject.Entities;
using DataAccess.Models;

namespace DataAccess.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly DormitoryDbContext _context;
        public PaymentRepository(DormitoryDbContext context)
        {
            _context = context;
        }
        public void AddPayment(Payment payment)
        {
            _context.Payments.Add(payment);
        }
        public async Task<Payment?> GetPaymentById(string paymentId)
        {
            return await _context.Payments.FindAsync(paymentId);
        }
        public void UpdatePayment(Payment payment)
        {
            _context.Payments.Update(payment);
        }
    }
}
