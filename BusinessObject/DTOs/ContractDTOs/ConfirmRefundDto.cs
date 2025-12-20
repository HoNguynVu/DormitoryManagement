namespace BusinessObject.DTOs.ContractDTOs
{
    public class ConfirmRefundDto
    {
        public string ReceiptId { get; set; } = string.Empty;
        public string RefundMethod { get; set; } = string.Empty; // "BankTransfer", "Cash", etc.
        public string? TransactionReference { get; set; } // Mã giao d?ch ngân hàng
        public string? ManagerNote { get; set; }
    }
}
