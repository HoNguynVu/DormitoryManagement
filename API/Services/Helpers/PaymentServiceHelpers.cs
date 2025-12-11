using API.Services.Helpers;
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
                { "embed_data", JsonConvert.SerializeObject(new { redirecturl = "" }) },
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
    }
}
