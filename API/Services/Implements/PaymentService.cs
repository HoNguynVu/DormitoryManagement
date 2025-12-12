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
