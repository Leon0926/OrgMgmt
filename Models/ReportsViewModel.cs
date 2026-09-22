namespace OrgMgmt.Models
{
    public class CityClientCount
    {
        public string City { get; set; } = string.Empty;
        public int ClientCount { get; set; }
    }

    public class ReportsViewModel
    {
        public List<CityClientCount> ClientsPerCity { get; set; } = new();

        public List<Client> ClientsWithoutServices { get; set; } = new();
    }
}
