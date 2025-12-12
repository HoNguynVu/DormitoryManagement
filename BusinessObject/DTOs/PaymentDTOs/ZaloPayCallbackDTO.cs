using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.PaymentDTOs
{
    public class ZaloPayCallbackDTO
    {
        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonPropertyName("mac")]
        public string Mac { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }
    }
}
