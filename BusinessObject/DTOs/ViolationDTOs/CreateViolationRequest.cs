using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.ViolationDTOs
{
    public class CreateViolationRequest
    {
        public string StudentId { get; set; }
        public string AccountId { get; set; }
        public string ViolationAct { get; set; }
        public string Description { get; set; }
    }
}
