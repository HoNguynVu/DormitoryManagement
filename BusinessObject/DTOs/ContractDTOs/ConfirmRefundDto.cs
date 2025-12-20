using BusinessObject.Entities;

namespace BusinessObject.DTOs.ContractDTOs
{
    public class ConfirmRefundDto
    {
        public string ReceiptId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string NewRoomId { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string RefundMethod { get; set; } = string.Empty; // "BankTransfer", "Cash", etc.
        public string? TransactionReference { get; set; } // Mã giao d?ch ngân hàng
        public string? ManagerNote { get; set; }
    }
}
