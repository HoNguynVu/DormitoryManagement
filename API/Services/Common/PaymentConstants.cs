namespace API.Services.Common
{
    public static class PaymentConstants
    {
        public const string MethodZaloPay = "ZaloPay";
        public const string MethodCash = "Cash";
        public const string MethodTransfer = "Transfer";

        public const string StatusPending = "Pending";
        public const string StatusSuccess = "Success";
        public const string StatusFailed = "Failed";

        public const string RegisPaid = "Paid";
        public const string RegisPending = "Pending";

        public const string PrefixRegis = "REG";

        public const string TypeRegis = "Registration";
    }
}