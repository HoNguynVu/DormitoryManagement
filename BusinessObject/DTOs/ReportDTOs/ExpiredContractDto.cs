namespace BusinessObject.DTOs.ReportDTOs
{
    public record ExpiredContractDto
    {
        public string ContractID { get; init; } = string.Empty;
        public string StudentID { get; init; } = string.Empty;
        public string StudentName { get; init; } = string.Empty;
        public string RoomID { get; init; } = string.Empty;
        public string RoomName { get; init; } = string.Empty;
        public DateOnly EndDate { get; init; }
        public string ContractStatus { get; init; } = string.Empty;
    }
}
