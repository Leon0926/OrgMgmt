namespace OrgMgmt.Models
{
    public class Client : Person
    {
        public decimal Balance { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
