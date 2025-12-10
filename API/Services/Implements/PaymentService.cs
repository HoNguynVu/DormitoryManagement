using BusinessObject.Config;

namespace API.Services.Implements
{
    public partial class PaymentService
    {
        private readonly ZaloPaySettings _zaloConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        public PaymentService(ZaloPaySettings zaloConfig,IHttpClientFactory httpClientFactory)
        {
            _zaloConfig = zaloConfig;
            _httpClientFactory = httpClientFactory;
        }
    }
}
