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
    public class ServicesController : Controller
    {
        private readonly OrgDbContext _context;

        public ServicesController(OrgDbContext context)
        {
            _context = context;
        }

        private async Task PopulateSelectListsAsync(Guid? employeeId = null, IEnumerable<Guid>? selectedClients = null)
        {
            ViewData["EmployeeID"] = new SelectList(
                await _context.Employees.OrderBy(e => e.Name).ToListAsync(), "ID", "Name", employeeId);
            ViewBag.ClientList = new MultiSelectList(
                await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "ID", "Name", selectedClients);
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Services
                .Include(s => s.Employee)
                .Include(s => s.Clients)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Employee)
                .Include(s => s.Clients)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateSelectListsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ID,Type,Rate,EmployeeID")] Service service,
            Guid[] selectedClients)
        {
            if (ModelState.IsValid)
            {
                service.ID = Guid.NewGuid();

                if (selectedClients != null && selectedClients.Length > 0)
                {
                    var clients = await _context.Clients
                        .Where(c => selectedClients.Contains(c.ID))
                        .ToListAsync();
                    foreach (var client in clients)
                    {
                        service.Clients.Add(client);
                    }
                }

                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateSelectListsAsync(service.EmployeeID, selectedClients);
            return View(service);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Clients)
                .FirstOrDefaultAsync(s => s.ID == id);
            if (service == null)
            {
                return NotFound();
            }

            await PopulateSelectListsAsync(service.EmployeeID, service.Clients.Select(c => c.ID));
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            [Bind("ID,Type,Rate,EmployeeID")] Service service,
            Guid[] selectedClients)
        {
            if (id != service.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var serviceToUpdate = await _context.Services
                    .Include(s => s.Clients)
                    .FirstOrDefaultAsync(s => s.ID == id);
                if (serviceToUpdate == null)
                {
                    return NotFound();
                }

                serviceToUpdate.Type = service.Type;
                serviceToUpdate.Rate = service.Rate;
                serviceToUpdate.EmployeeID = service.EmployeeID;

                serviceToUpdate.Clients.Clear();
                if (selectedClients != null && selectedClients.Length > 0)
                {
                    var clients = await _context.Clients
                        .Where(c => selectedClients.Contains(c.ID))
                        .ToListAsync();
                    foreach (var client in clients)
                    {
                        serviceToUpdate.Clients.Add(client);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ID))
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

            await PopulateSelectListsAsync(service.EmployeeID, selectedClients);
            return View(service);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Employee)
                .Include(s => s.Clients)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var service = await _context.Services
                .Include(s => s.Clients)
                .FirstOrDefaultAsync(s => s.ID == id);
            if (service != null)
            {
                service.Clients.Clear();
                _context.Services.Remove(service);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(Guid id)
        {
            return _context.Services.Any(e => e.ID == id);
        }
    }
}
