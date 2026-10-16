using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrgMgmt.Models
{
    public class Person
    {
        public Guid ID { get; set; }

        [StringLength(20, ErrorMessage = "Name cannot be longer than 20 characters.")]
        [RegularExpression(@"^[^0-9]*$", ErrorMessage = "Name cannot contain numbers.")]
        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        public byte[]? Photo { get; set; }

        public string? PhotoContentType { get; set; }

        /// <summary>Uploaded file bound from the Create/Edit form; never stored.</summary>
        [NotMapped]
        [Display(Name = "Photo")]
        public IFormFile? PhotoFile { get; set; }

        /// <summary>Set by the "Remove photo" checkbox on the Edit form; never stored.</summary>
        [NotMapped]
        [Display(Name = "Remove photo")]
        public bool RemovePhoto { get; set; }

        [NotMapped]
        public bool HasPhoto => Photo != null && Photo.Length > 0;
    }
}
