using API.Services.Common;
using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Config;
using BusinessObject.DTOs;
using BusinessObject.DTOs.PaymentDTOs;
using BusinessObject.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace API.Services.Implements
{
    public partial class PaymentService : IPaymentService
    {
        private readonly ZaloPaySettings _zaloConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPaymentUow _paymentUow;
        private readonly ILogger<PaymentService> _logger;
        private readonly IHealthInsuranceService _healthInsuranceService;
        private readonly IRegistrationService _registrationService;
        private readonly IContractService _contractService;
        private readonly IUtilityBillService _utilityBillService;
        public PaymentService(IOptions<ZaloPaySettings> zaloConfig,
            IHttpClientFactory httpClientFactory, 
            IPaymentUow paymentUow, 
            ILogger<PaymentService> logger,
            IHealthInsuranceService healthInsuranceService,
            IRegistrationService registrationService,
            IContractService contractService,
            IUtilityBillService utilityBillService)
        {
            _zaloConfig = zaloConfig.Value;
            _httpClientFactory = httpClientFactory;
            _paymentUow = paymentUow;
            _healthInsuranceService = healthInsuranceService;
            _registrationService = registrationService;
            _contractService = contractService;
            _utilityBillService = utilityBillService;
            _logger = logger;
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
                return (200, new PaymentLinkDTO { IsSuccess = true, PaymentId = appTransId, PaymentUrl = orderUrl, Message = "ZaloPay payment link created successfully." });
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
            if (receipt.PaymentType != PaymentConstants.TypeRenewal)
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
                    PaymentId = appTransId,
                    Message = "Renewal payment link created successfully."
                });
            }
            catch (Exception ex)
            {
                await _paymentUow.RollbackAsync();
                return (500, new PaymentLinkDTO { IsSuccess = false, Message = $"Database error: {ex.Message}" });
            }
        }

        public async Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForUtility(string utilityId, string accountId)
        {
            // Validate
            if (string.IsNullOrEmpty(utilityId))
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Invalid utility ID" });
            }
            if (string.IsNullOrEmpty(accountId))
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Invalid student ID" });
            }

            var student = await _paymentUow.Students.GetStudentByAccountIdAsync(accountId);
            if (student == null)
            {
                return (404, new PaymentLinkDTO { IsSuccess = false, Message = "Student not found" });
            }
            var utilityBill = await _paymentUow.UtilityBills.GetByIdAsync(utilityId);
            if (utilityBill == null)
            {
                return (404, new PaymentLinkDTO { IsSuccess = false, Message = "Utility bill not found" });
            }
            if (utilityBill.Status != "Unpaid")
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Utility bill is not in unpaid status" });
            }


            var contract = await _paymentUow.Contracts.GetActiveContractByStudentId(student.StudentID);

            // Check 1: SV có hợp đồng không?
            if (contract == null)
            {
                return (403, new PaymentLinkDTO { IsSuccess = false, Message = "Student does not have an active contract." });
            }

            // Check 2: SV có ở đúng cái phòng của hóa đơn điện nước này không?
            if (contract.RoomID != utilityBill.RoomID)
            {
                return (403, new PaymentLinkDTO { IsSuccess = false, Message = "You do not live in the room associated with this bill." });
            }

            var amount = utilityBill.Amount;
            var appTransId = GenerateAppTransId(PaymentConstants.PrefixUtility, utilityId);
            string description = $"Thanh toan hoa don tien dien nuoc {utilityId} ";
            string orderUrl = await CallZaloPayCreateOrder(appTransId, (long)amount, description, utilityId);
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
                    StudentID = student.StudentID,
                    Amount = amount,
                    RelatedObjectID = utilityId,
                    PaymentType = PaymentConstants.TypeUtility,
                    Status = PaymentConstants.StatusPending,
                    PrintTime = DateTime.Now,
                    Content = $"Payment for utility bill {utilityId}"
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
                return (200, new PaymentLinkDTO
                {
                    IsSuccess = true,
                    PaymentUrl = orderUrl,
                    PaymentId = appTransId,
                    Message = "ZaloPay payment link created successfully."
                });
            }
            catch (Exception ex)
            {
                await _paymentUow.RollbackAsync();
                return (500, new PaymentLinkDTO { IsSuccess = false, Message = $"Failed to create ZaloPay payment link: {ex.Message}" });
            }
        }

        public async Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForHealthInsurance(string insuranceId)
        {
            // Validate 
            if (string.IsNullOrEmpty(insuranceId))
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Invalid insurance ID" });
            }

            var form = await _paymentUow.HealthInsurances.GetByIdAsync(insuranceId);
            if (form == null)
            {
                return (404, new PaymentLinkDTO { IsSuccess = false, Message = "Health insurance form not found" });
            }
            if (form.Status != "Pending")
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Health insurance form is not in pending status" });
            }
            var year = DateTime.Now.Year;
            var amount = form.Cost;
            var appTransId = GenerateAppTransId(PaymentConstants.PrefixHealthInsurance, insuranceId);
            string description = $"Thanh toan bao hiem y te {insuranceId} nam {year} ";
            string orderUrl = await CallZaloPayCreateOrder(appTransId, (long)amount, description, insuranceId);
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
                    RelatedObjectID = insuranceId,
                    PaymentType = PaymentConstants.TypeHealthInsurance,
                    Status = PaymentConstants.StatusPending,
                    PrintTime = DateTime.Now,
                    Content = $"Payment for health insurance form {insuranceId}"
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
                return (200, new PaymentLinkDTO
                {
                    IsSuccess = true,
                    PaymentUrl = orderUrl,
                    PaymentId = appTransId,
                    Message = "ZaloPay payment link created successfully."
                });
            }
            catch (Exception ex)
            {
                await _paymentUow.RollbackAsync();
                return (500, new PaymentLinkDTO { IsSuccess = false, Message = $"Failed to create ZaloPay payment link: {ex.Message}" });
            }
        }
        public async Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForRoomChange(string receiptId)
        {
            // 1. Validate Input
            if (string.IsNullOrEmpty(receiptId))
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Receipt ID is required." });
            }

            // 2. Get Receipt
            var receipt = await _paymentUow.Receipts.GetByIdAsync(receiptId);
            if (receipt == null)
            {
                return (404, new PaymentLinkDTO { IsSuccess = false, Message = "Receipt not found." });
            }

            // 3. Validate receipt type and status
            if (receipt.PaymentType != PaymentConstants.TypeRoomChange)
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Invalid receipt type. Expected RoomChangeCharge." });
            }

            if (receipt.Status != PaymentConstants.StatusPending)
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "This receipt is not pending (already paid or cancelled)." });
            }

            // 4. Prepare ZaloPay data
            var amount = receipt.Amount;
            var appTransId = GenerateAppTransId(PaymentConstants.PrefixRoomChange, receiptId);
            string description = receipt.Content ?? $"Thanh toan phi doi phong {receipt.RelatedObjectID}";

            // 5. Call ZaloPay API
            string orderUrl = await CallZaloPayCreateOrder(appTransId, (long)amount, description, receiptId);
            if (string.IsNullOrEmpty(orderUrl))
            {
                return (500, new PaymentLinkDTO { IsSuccess = false, Message = "Failed to create ZaloPay order." });
            }

            // 6. Save Payment record
            await _paymentUow.BeginTransactionAsync();
            try
            {
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

                _paymentUow.Payments.Add(payment);
                await _paymentUow.CommitAsync();

                return (200, new PaymentLinkDTO
                {
                    IsSuccess = true,
                    PaymentUrl = orderUrl,
                    PaymentId = appTransId,
                    Message = "Room change payment link created successfully."
                });
            }
            catch (Exception ex)
            {
                await _paymentUow.RollbackAsync();
                return (500, new PaymentLinkDTO { IsSuccess = false, Message = $"Database error: {ex.Message}" });
            }
        }

        public async Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForMaintenance(string receiptId)
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
            if (receipt.PaymentType != PaymentConstants.TypeMaintenanceFee)
            {
                return (400, new PaymentLinkDTO { IsSuccess = false, Message = "Invalid receipt type. Expected Renewal." });
            }

            // 4. Chuẩn bị dữ liệu ZaloPay
            var amount = receipt.Amount;
            var appTransId = GenerateAppTransId(PaymentConstants.PrefixRenew, receiptId);
            string description = receipt.Content ?? $"Thanh toan phi sua chua {receipt.RelatedObjectID}";

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
                    PaymentId = appTransId,
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
                    return (-1, "Dữ lSiệu callback không hợp lệ.");
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
                else if (appTransId.Contains($"_{PaymentConstants.PrefixRenew}_"))
                {
                    var result = await HanldeRenewalSuccessPayment(appTransId, zpTransId);

                    // 2. Kiểm tra kết quả
                    if (!result.Success)
                    {
                        return (0, $"Lỗi xử lý gia hạn: {result.Message}");
                    }
                }
                else if (appTransId.Contains($"_{PaymentConstants.PrefixHealthInsurance}_"))
                {
                    var result = await HanldeInsuranceSuccessPayment(appTransId, zpTransId);
                    if (!result.Success)
                    {
                        return (0, $"Lỗi xử lý gia hạn: {result.Message}");
                    }
                }
                else if (appTransId.Contains($"_{PaymentConstants.PrefixRoomChange}_"))
                {
                    var result = await HandleRoomChangeSuccessPayment(appTransId, zpTransId);
                    if (!result.Success)
                    {
                        return (0, $"Lỗi xử lý gia hạn: {result.Message}");
                    }
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
