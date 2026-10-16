namespace OrgMgmt.Models
{
    public class Employee : Person
    {
        public decimal Salary { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
