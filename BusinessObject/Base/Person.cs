using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Base
{
    // Abstract để EF Core không tạo bảng Persons
    public abstract class Person
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        // Property này sẽ được map vào các cột tên khác nhau (Address hoặc CurrentAddress)
        [StringLength(500)]
        public virtual string Address { get; set; }
    }
}