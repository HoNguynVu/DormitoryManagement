using API.Services.Common;
using API.Services.Helpers;
using BusinessObject.DTOs.RegisDTOs;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.Services.Implements
{
    public partial class PaymentService
    {
        //Tạo mã giao dịch
        private string GenerateAppTransId(string prefix, string refId)
        {
            // Format: yyMMdd_PREFIX_xxxx (Ví dụ: 231201_INV_1234)
            var rnd = new Random();
            return $"{DateTime.Now:yyMMdd}_{prefix}_{rnd.Next(1000, 9999)}";
        }

        // Gọi ZaloPay API để tạo đơn hàng
        private async Task<string> CallZaloPayCreateOrder(string appTransId, long amount, string description, string itemId)
        {
            var rnd = new Random();
            var items = new[] { new { item_id = itemId, item_name = description } };
            long appTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var param = new Dictionary<string, string>
            {
                { "app_id", _zaloConfig.AppId },
                { "app_user", "HotelSystem" },
                { "app_time", appTime.ToString() },
                { "amount", amount.ToString() },
                { "app_trans_id", appTransId },
                { "embed_data", JsonConvert.SerializeObject(new { redirecturl = _zaloConfig.FrontEndUrl }) },
                { "item", JsonConvert.SerializeObject(items) },
                { "description", description },
                { "bank_code", "" },
                { "callback_url", _zaloConfig.CallbackUrl } // URL Ngrok
            };

            // Tạo chữ ký (Key1)
            string data = $"{_zaloConfig.AppId}|{appTransId}|HotelSystem|{amount}|{appTime}|{param["embed_data"]}|{param["item"]}";
            param.Add("mac", ZaloPayHelper.HmacSHA256(data, _zaloConfig.Key1));

            var content = new FormUrlEncodedContent(param);
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.PostAsync(_zaloConfig.CreateOrderUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseString))
            {
                throw new Exception("Không nhận được phản hồi từ ZaloPay (Empty Response).");
            }

            dynamic? responseData = JsonConvert.DeserializeObject(responseString);
            if (responseData == null)
                throw new Exception("Lỗi phản hồi từ ZaloPay.");

            if (responseData.return_code == 1)
            {
                return responseData.order_url;
            }
            else
            {
                // 👇 In thêm sub_return_code và sub_return_message để biết chi tiết
                string debugInfo = $"ReturnCode: {responseData.return_code}, Msg: {responseData.return_message}, SubCode: {responseData.sub_return_code}, SubMsg: {responseData.sub_return_message}";

                // Đặt breakpoint ở đây để xem debugInfo là gì
                throw new Exception($"ZaloPay Error: {debugInfo}");
            }
        }

        private async Task<(bool Success, string Message, int StatusCode)> HandleRegisSuccessPayment(string appTransId, string zpTransId)
        {
            return await ExecutePaymentTransaction(appTransId, zpTransId, async (receipt) =>
            {
                return await _registrationService.ConfirmPaymentForRegistration(receipt.RelatedObjectID);
            });
        }

        private async Task<(bool Success, string Message, int StatusCode)> HandleRenewalSuccessPayment(string appTransId, string zpTransId)
        {
            return await ExecutePaymentTransaction(appTransId, zpTransId, async (receipt) =>
            {
                var contract = await _paymentUow.Contracts.GetDetailContractAsync(receipt.RelatedObjectID);

                // 2. Kiểm tra dữ liệu Hợp đồng và Phòng
                if (contract == null)
                    return (false, "Contract not found.", 404);

                if (contract.Room?.RoomType == null)
                    return (false, "Room configuration is missing.", 500);

                // 3. Lấy giá và kiểm tra an toàn
                decimal? rawPrice = contract.Room.RoomType.Price;

                if (!rawPrice.HasValue || rawPrice.Value == 0)
                {
                    return (false, "Room price is invalid (zero or not set). Cannot calculate extension.", 500);
                }

                // 4. Tính toán số tháng (Dùng .Value để lấy giá trị thực)
                int months = (receipt.Amount == rawPrice) ? 12 : 6;

                if (months <= 0)
                {
                    return (false, "Payment amount is not enough for 1 month extension.", 400);
                }

                return await _contractService.ConfirmContractExtensionAsync(receipt.RelatedObjectID, months);
            });
        }

        private async Task<(bool Success, string Message, int StatusCode)> HandleUtilitySuccessPayment(string appTransId, string zpTransId)
        {
            return await ExecutePaymentTransaction(appTransId, zpTransId, async (receipt) =>
            {

                return await _utilityBillService.ConfirmUtilityPaymentAsync(receipt.RelatedObjectID);
            });
        }

        private async Task<(bool Success, string Message, int StatusCode)> HandleInsuranceSuccessPayment(string appTransId, string zpTransId)
        {
            return await ExecutePaymentTransaction(appTransId, zpTransId, async (receipt) =>
            {
                return await _healthInsuranceService.ConfirmInsurancePaymentAsync(receipt.RelatedObjectID);
            });
        }

        private async Task<(bool Success, string Message, int StatusCode)> HanldeMaintenanceSuccessPayment(string appTransId, string zpTransId)
        {
            return await ExecutePaymentTransaction(appTransId, zpTransId, async (receipt) =>
            {
                return await _maintenanceService.ConfirmPaymentMaintenanceFee(receipt.RelatedObjectID);
            });
        }

        private async Task<(bool Success, string Message, int StatusCode)> HandleRoomChangeSuccessPayment(string appTransId, string zpTransId)
        {
            return await ExecutePaymentTransaction(appTransId, zpTransId, async (receipt) =>
            {
                string newRoomId = null;
                if (!string.IsNullOrEmpty(receipt.Content) && receipt.Content.Contains("CMD|"))
                {
                    var parts = receipt.Content.Split(new string[] { "CMD|" }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        var dataParts = parts[1].Split('|');
                        if (dataParts.Length > 0)
                        {
                            newRoomId = dataParts[0].Trim();
                        }
                    }
                }
                if (string.IsNullOrEmpty(newRoomId))
                {
                    // Log error here
                    return (false, "Error: No Room information in Receipt.", 400);
                }
                var activeContract = await _contractUow.Contracts.GetByIdAsync(receipt.RelatedObjectID);
                if (activeContract == null)
                {
                    return (false, "Error: No contract found.", 404);
                }

                try
                {
                    await RoomTransactionHelper.SwapRoomLogicAsync(_contractUow, activeContract, newRoomId);

                    return (true, "Room change payment confirmed successfully.", 200);
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi cập nhật phòng: {ex.Message}", 500);
                }
          

                return  (true, "Room change payment confirmed successfully.", 200);
            });
        }

        // Hàm Generic xử lý chung cho mọi loại thanh toán
        private async Task<(bool Success, string Message, int StatusCode)> ExecutePaymentTransaction(
            string appTransId,
            string zpTransId,
            Func<Receipt, Task<(bool Success, string Message, int StatusCode)>> businessLogic)
        {
            await _paymentUow.BeginTransactionAsync();
            try
            {
                // 1. Tìm Payment
                var payment = await _paymentUow.Payments.GetByIdAsync(appTransId);
                if (payment == null)
                    return (false, $"Payment not found for AppTransId: {appTransId}", 404);

                // 2. Tìm Receipt
                var receipt = await _paymentUow.Receipts.GetByIdAsync(payment.ReceiptID);
                if (receipt == null)
                    return (false, "Receipt not found", 404);

                // 3. Kiểm tra Idempotency (Nếu đã Paid thì return luôn)
                if (receipt.Status == PaymentConstants.StatusSuccess)
                    return (true, "Transaction already processed successfully.", 200);

                // 4. CHẠY LOGIC NGHIỆP VỤ RIÊNG (Registration, Contract, Insurance...)
                var logicResult = await businessLogic(receipt);

                // 5. Nếu nghiệp vụ thành công -> Cập nhật Receipt & Payment
                if (logicResult.Success)
                {
                    payment.Status = PaymentConstants.StatusSuccess;
                    payment.TransactionID = zpTransId; // Lưu mã ZaloPay

                    receipt.Status = PaymentConstants.StatusSuccess;
                    receipt.PrintTime = DateTime.Now;

                    _paymentUow.Payments.Update(payment);
                    _paymentUow.Receipts.Update(receipt);

                    await _paymentUow.CommitAsync();
                }

                return logicResult;
            }
            catch (Exception ex)
            {
                return (false, $"System Error: {ex.Message}", 500);
            }
        }
    }
}
