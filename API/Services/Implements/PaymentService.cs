using API.Services.Common;
using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Config;
using BusinessObject.DTOs;
using BusinessObject.DTOs.PaymentDTOs;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public partial class PaymentService : IPaymentService
    {
        private readonly ZaloPaySettings _zaloConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPaymentUow _paymentUow;
        public PaymentService(ZaloPaySettings zaloConfig,IHttpClientFactory httpClientFactory, IPaymentUow paymentUow)
        {
            _zaloConfig = zaloConfig;
            _httpClientFactory = httpClientFactory;
            _paymentUow = paymentUow;
        }

        public async Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForRegistration(string registrationId)
        {
            var form = await _paymentUow.RegistrationForms.GetByIdAsync(registrationId);
            if (form == null)
            {
                return (404, new PaymentLinkDTO { IsSuccess = false, Message = "Registration form not found" });
            }
            if (form.Status != "Pending")
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Registration form is not in pending status" });
            }
            var roomType = await _paymentUow.RoomTypes.GetRoomTypeByRoomId(form.RoomId);
            var amount = roomType.Price;
            var appTransId = GenerateAppTransId(PaymentConstants.PrefixRegis, registrationId);
            string description = $"Thanh toan dang ky o {registrationId}";
            string orderUrl = await CallZaloPayCreateOrder(appTransId, (long)amount, description, registrationId);
            await _paymentUow.BeginTransactionAsync();
            try
            {
                var receipt = new Receipt
                {
                    ReceiptId = "REC-" + IdGenerator.GenerateUniqueSuffix(),
                    StudentId = form.StudentId,
                    Amount = amount,
                    RelatedObjectId = registrationId,
                    PaymentType = PaymentConstants.TypeRegis,
                    Status = PaymentConstants.StatusPending,
                    PrintTime = DateTime.Now,
                    Content = $"Payment for registration form {registrationId}"
                };
                var payment = new Payment
                {
                    PaymentId = appTransId,
                    PaymentMethod = PaymentConstants.MethodZaloPay,
                    Amount = amount,
                    TransactionId = appTransId,
                    Status = PaymentConstants.StatusPending,
                    PaymentDate = DateTime.Now,
                    ReceiptId = receipt.ReceiptId,
                };
                _paymentUow.Receipts.AddReceipt(receipt);
                _paymentUow.Payments.AddPayment(payment);
                await _paymentUow.CommitAsync();
                return (200, new PaymentLinkDTO { IsSuccess = true, PaymentUrl = orderUrl, Message = "ZaloPay payment link created successfully." });
            }
            catch (Exception ex)
            {
                await _paymentUow.RollbackAsync();
                return (500, new PaymentLinkDTO { IsSuccess = false, Message = $"Failed to create ZaloPay payment link: {ex.Message}" });
            }
        }
    }
}
