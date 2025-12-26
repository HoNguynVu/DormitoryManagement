namespace BusinessObject.DTOs.ViolationDTOs
{
    public class ViolationResponse
    {
        public string ViolationId { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string? ReportingManagerId { get; set; }
        public string? ReportingManagerName { get; set; }
        public string? RoomId { get; set; }
        public string? RoomName { get; set; }
        public string ViolationAct { get; set; }
        public DateTime ViolationTime { get; set; }
        public string? Description { get; set; }
        public string? Resolution { get; set; }
        public int TotalViolationsOfStudent { get; set; }

    }
}
