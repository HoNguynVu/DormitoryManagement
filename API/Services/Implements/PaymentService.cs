using API.Services.Common;
using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Config;
using BusinessObject.DTOs;
using BusinessObject.DTOs.PaymentDTOs;
using BusinessObject.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace API.Services.Implements
{
    public partial class PaymentService : IPaymentService
    {
        private readonly ZaloPaySettings _zaloConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPaymentUow _paymentUow;

        private readonly IHealthInsuranceService _healthInsuranceService;
        private readonly IRegistrationService _registrationService;
        private readonly IContractService _contractService;
        public PaymentService(IOptions<ZaloPaySettings> zaloConfig,
            IHttpClientFactory httpClientFactory, 
            IPaymentUow paymentUow, 
            IHealthInsuranceService healthInsuranceService,
            IRegistrationService registrationService,
            IContractService contractService)
        {
            _zaloConfig = zaloConfig.Value;
            _httpClientFactory = httpClientFactory;
            _paymentUow = paymentUow;
            _healthInsuranceService = healthInsuranceService;
            _registrationService = registrationService;
            _contractService = contractService;
        }

        public async Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForRegistration(string registrationId)
        {
            // Validate registration form
            if (string.IsNullOrEmpty(registrationId))
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Invalid registration ID" });
            }

            var form = await _paymentUow.RegistrationForms.GetByIdAsync(registrationId);
            if (form == null)
            {
                return (404, new PaymentLinkDTO { IsSuccess = false, Message = "Registration form not found" });
            }
            if (form.Status != "Pending")
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Registration form is not in pending status" });
            }
            var roomType = await _paymentUow.RoomTypes.GetRoomTypeByRoomId(form.RoomID);
            if (roomType == null)
            {
                return (404, new PaymentLinkDTO { IsSuccess = false, Message = "Room type not found for the registration form" });
            }

            var amount = roomType.Price;
            var appTransId = GenerateAppTransId(PaymentConstants.PrefixRegis, registrationId);
            string description = $"Thanh toan dang ky o {registrationId}";

            string orderUrl = await CallZaloPayCreateOrder(appTransId, (long)amount, description, registrationId);
            if (string.IsNullOrEmpty(orderUrl))
            {
                return (500, new PaymentLinkDTO { IsSuccess = false, Message = "Failed to create ZaloPay order" });
            }
            await _paymentUow.BeginTransactionAsync();
            try
            {
                var receipt = new Receipt
                {
                    ReceiptID = "RE-" + IdGenerator.GenerateUniqueSuffix(),
                    StudentID = form.StudentID,
                    Amount = amount,
                    RelatedObjectID = registrationId,
                    PaymentType = PaymentConstants.TypeRegis,
                    Status = PaymentConstants.StatusPending,
                    PrintTime = DateTime.Now,
                    Content = $"Payment for registration form {registrationId}"
                };
                var payment = new Payment
                {
                    PaymentID = appTransId,
                    PaymentMethod = PaymentConstants.MethodZaloPay,
                    Amount = amount,
                    ReceiptID = receipt.ReceiptID,
                    TransactionID = appTransId,
                    Status = PaymentConstants.StatusPending,
                    PaymentDate = DateTime.Now,
                };
                _paymentUow.Receipts.Add(receipt);
                _paymentUow.Payments.Add(payment);
                await _paymentUow.CommitAsync();
                return (200, new PaymentLinkDTO { IsSuccess = true, PaymentUrl = orderUrl, Message = "ZaloPay payment link created successfully." });
            }
            catch (Exception ex)
            {
                await _paymentUow.RollbackAsync();
                return (500, new PaymentLinkDTO { IsSuccess = false, Message = $"Failed to create ZaloPay payment link: {ex.Message}" });
            }
        }

        public async Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForRenewal(string receiptId)
        {
            // 1. Validate Input
            if (string.IsNullOrEmpty(receiptId))
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Receipt ID is required." });
            }

            // 2. Lấy thông tin Receipt 
            var receipt = await _paymentUow.Receipts.GetByIdAsync(receiptId);

            if (receipt == null)
            {
                return (404, new PaymentLinkDTO { IsSuccess = false, Message = "Receipt not found." });
            }

            // 3. Kiểm tra hợp lệ
            // Chỉ thanh toán cho Receipt trạng thái Pending và loại là Renewal
            if (receipt.Status != "Pending")
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "This receipt is not pending (already paid or cancelled)." });
            }

            // Kiểm tra đúng loại hóa đơn gia hạn không 
            if (receipt.PaymentType != "RenewalContract")
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Invalid receipt type. Expected Renewal." });
            }

            // 4. Chuẩn bị dữ liệu ZaloPay
            var amount = receipt.Amount;
            var appTransId = GenerateAppTransId(PaymentConstants.PrefixRenew, receiptId);
            string description = receipt.Content ?? $"Thanh toan gia han hop dong {receipt.RelatedObjectID}";

            // 5. Gọi ZaloPay API
            // Lưu ý: embed_data = receiptId để khi Callback biết update Receipt nào
            string orderUrl = await CallZaloPayCreateOrder(appTransId, (long)amount, description, receiptId);

            if (string.IsNullOrEmpty(orderUrl))
            {
                return (500, new PaymentLinkDTO { IsSuccess = false, Message = "Failed to create ZaloPay order." });
            }

            // 6. Lưu thông tin Payment (Ghi nhận là SV đang cố gắng thanh toán)
            await _paymentUow.BeginTransactionAsync();
            try
            {
                var payment = new Payment
                {
                    PaymentID = appTransId, // Dùng luôn mã của ZaloPay làm ID
                    PaymentMethod = PaymentConstants.MethodZaloPay,
                    Amount = amount,
                    ReceiptID = receipt.ReceiptID, // Link ngược về Receipt
                    TransactionID = appTransId,
                    Status = PaymentConstants.StatusPending,
                    PaymentDate = DateTime.Now,
                };

                _paymentUow.Payments.Add(payment);
                await _paymentUow.CommitAsync();

                return (200, new PaymentLinkDTO
                {
                    IsSuccess = true,
                    PaymentUrl = orderUrl,
                    Message = "Renewal payment link created successfully."
                });
            }
            catch (Exception ex)
            {
                await _paymentUow.RollbackAsync();
                return (500, new PaymentLinkDTO { IsSuccess = false, Message = $"Database error: {ex.Message}" });
            }
        }
        public async Task<(int ReturnCode, string ReturnMessage)> ProcessZaloPayCallback(ZaloPayCallbackDTO cbdata)
        {
            try
            {
                // 1. Validate Input
                if (cbdata == null || string.IsNullOrEmpty(cbdata.Data) || string.IsNullOrEmpty(cbdata.Mac))
                {
                    return (-1, "Dữ liệu callback không hợp lệ.");
                }

                string dataStr = cbdata.Data;
                string reqMac = cbdata.Mac;

                // 2. Kiểm tra Chữ ký (MAC)
                string mac = ZaloPayHelper.HmacSHA256(dataStr, _zaloConfig.Key2);

                if (!mac.Equals(reqMac))
                {
                    return (-1, "Chữ ký MAC không hợp lệ.");
                }

                // 3. Parse Data
                var dataJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(dataStr);
                if (dataJson == null)
                {
                    return (0, "Không thể giải mã dữ liệu JSON.");
                }

                string appTransId = dataJson["app_trans_id"];
                string zpTransId = dataJson["zp_trans_id"];

                // 4. Xử lý logic nghiệp vụ

                if (appTransId.Contains($"_{PaymentConstants.PrefixRegis}_"))
                {
                    await HanldeRegisSuccessPayment(appTransId, zpTransId);
                }
                else if (appTransId.Contains($"_{PaymentConstants.PrefixUtility}_"))
                {
                    
                    await HanldeUtilitySuccessPayment(appTransId, zpTransId);
                }
                else if (appTransId.Contains($"_{PaymentConstants.PrefixContract}_"))
                {
                    await HanldeRenewalSuccessPayment(appTransId, zpTransId);
                }
                else if (appTransId.Contains($"_{PaymentConstants.PrefixHealthInsurance}_"))
                {
                    await HanldeInsuranceSuccessPayment(appTransId, zpTransId);
                }
                else
                {
                    return (0, "Loại giao dịch không xác định.");
                }
                return (1, "success");
            }
            catch (Exception ex)
            {
                return (0, $"Lỗi hệ thống xử lý : {ex.Message}");
            }
        }

        

    }
}
