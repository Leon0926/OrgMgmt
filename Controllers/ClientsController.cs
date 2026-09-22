using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrgMgmt;
using OrgMgmt.Models;

namespace OrgMgmt.Controllers
{
    public class ClientsController : Controller
    {
        private readonly OrgDbContext _context;

        public ClientsController(OrgDbContext context)
        {
            _context = context;
        }

        private async Task PopulateServiceListAsync(IEnumerable<Guid>? selected = null)
        {
            var services = await _context.Services.OrderBy(s => s.Type).ToListAsync();
            ViewBag.ServiceList = new MultiSelectList(services, "ID", "Type", selected);
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Clients
                .Include(c => c.Services)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Services)
                    .ThenInclude(s => s.Employee)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateServiceListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Balance,ID,Name,Address,DateOfBirth,Photo")] Client client,
            Guid[] selectedServices)
        {
            if (ModelState.IsValid)
            {
                client.ID = Guid.NewGuid();

                if (selectedServices != null && selectedServices.Length > 0)
                {
                    var services = await _context.Services
                        .Where(s => selectedServices.Contains(s.ID))
                        .ToListAsync();
                    foreach (var service in services)
                    {
                        client.Services.Add(service);
                    }
                }

                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateServiceListAsync(selectedServices);
            return View(client);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.ID == id);
            if (client == null)
            {
                return NotFound();
            }

            await PopulateServiceListAsync(client.Services.Select(s => s.ID));
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            [Bind("Balance,ID,Name,Address,DateOfBirth,Photo")] Client client,
            Guid[] selectedServices)
        {
            if (id != client.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var clientToUpdate = await _context.Clients
                    .Include(c => c.Services)
                    .FirstOrDefaultAsync(c => c.ID == id);
                if (clientToUpdate == null)
                {
                    return NotFound();
                }

                clientToUpdate.Name = client.Name;
                clientToUpdate.Address = client.Address;
                clientToUpdate.DateOfBirth = client.DateOfBirth;
                clientToUpdate.Photo = client.Photo;
                clientToUpdate.Balance = client.Balance;

                clientToUpdate.Services.Clear();
                if (selectedServices != null && selectedServices.Length > 0)
                {
                    var services = await _context.Services
                        .Where(s => selectedServices.Contains(s.ID))
                        .ToListAsync();
                    foreach (var service in services)
                    {
                        clientToUpdate.Services.Add(service);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateServiceListAsync(selectedServices);
            return View(client);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Services)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var client = await _context.Clients
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.ID == id);
            if (client != null)
            {
                client.Services.Clear();
                _context.Clients.Remove(client);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(Guid id)
        {
            return _context.Clients.Any(e => e.ID == id);
        }
    }
}
