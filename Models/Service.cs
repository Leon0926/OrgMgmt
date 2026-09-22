using System.ComponentModel.DataAnnotations;

namespace OrgMgmt.Models
{
    public class Service
    {
        public Guid ID { get; set; }
        public string Type { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "The service rate cannot be a negative number.")]
        public decimal Rate { get; set; }

        public Guid? EmployeeID { get; set; }
        public Employee? Employee { get; set; }

        public ICollection<Client> Clients { get; set; } = new List<Client>();
    }
}
