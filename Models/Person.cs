using System.ComponentModel.DataAnnotations;

namespace OrgMgmt.Models
{
    public class Person
    {
        public Guid ID { get; set; }

        [StringLength(20, ErrorMessage = "Name cannot be longer than 20 characters.")]
        [RegularExpression(@"^[^0-9]*$", ErrorMessage = "Name cannot contain numbers.")]
        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public byte[]? Photo { get; set; }
    }
}
