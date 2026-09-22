using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrgMgmt.Models;

namespace OrgMgmt.Controllers
{
    public class ReportsController : Controller
    {
        private readonly OrgDbContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(OrgDbContext context, ILogger<ReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var clientsPerCity = await _context.Clients
                .GroupBy(c => c.Address)
                .Select(g => new CityClientCount
                {
                    City = g.Key,
                    ClientCount = g.Count()
                })
                .OrderBy(r => r.City)
                .ToListAsync();

            var clientsWithoutServices = await _context.Clients
                .Where(c => !c.Services.Any())
                .OrderBy(c => c.Name)
                .ToListAsync();

            _logger.LogInformation("Query 1 - clients per city (ordered by city):");
            foreach (var row in clientsPerCity)
            {
                _logger.LogInformation("    {City}: {Count}", row.City, row.ClientCount);
            }

            _logger.LogInformation("Query 2 - clients not registered for any service:");
            foreach (var client in clientsWithoutServices)
            {
                _logger.LogInformation("    {Name} ({City})", client.Name, client.Address);
            }

            return View(new ReportsViewModel
            {
                ClientsPerCity = clientsPerCity,
                ClientsWithoutServices = clientsWithoutServices
            });
        }
    }
}
