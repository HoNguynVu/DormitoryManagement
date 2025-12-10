using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.ViolationDTOs
{
    public class UpdateViolationRequest
    {
        public string ViolationId { get; set; }
        public string Resolution { get; set; }
    }
}
