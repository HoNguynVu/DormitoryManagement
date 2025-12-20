using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Config
{
    public class ZaloPaySettings
    {
        public string AppId { get; set; } = string.Empty;
        public string Key1 { get; set; } = string.Empty;
        public string Key2 { get; set; } = string.Empty;
        public string CreateOrderUrl { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
        public string FrontEndUrl { get; set; }

    }
}
