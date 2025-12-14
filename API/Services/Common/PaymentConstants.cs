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
        public const string PrefixUtility = "UTL";
        public const string PrefixHealthInsurance = "HI";
        public const string PrefixContract = "CNT";
        public const string PrefixRenew = "REN";


        public const string TypeRegis = "Registration";
        public const string TypeRenewal = "RenewalContract";
        public const string TypeUtility = "Utility";
        public const string TypeHealthInsurance = "HealthInsurance";
        public const string TypeMaintenanceFee = "MaintenanceFee";
        public const string TypeRenew = "RenewContract"; 
    }
}