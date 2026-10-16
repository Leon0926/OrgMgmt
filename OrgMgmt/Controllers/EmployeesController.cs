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
    public class EmployeesController : Controller
    {
        private readonly OrgDbContext _context;

        public EmployeesController(OrgDbContext context)
        {
            _context = context;
        }

        private async Task PopulateServiceListAsync(IEnumerable<Guid>? selected = null)
        {
            var services = await _context.Services.OrderBy(s => s.Type).ToListAsync();
            ViewBag.ServiceList = new MultiSelectList(services, "ID", "Type", selected);
        }

        /// <summary>
        /// Points the selected services at this employee and releases the ones no longer selected.
        /// A service has at most one employee, so selecting it here reassigns it.
        /// </summary>
        private async Task AssignServicesAsync(Employee employee, Guid[]? selectedServices)
        {
            var selected = selectedServices ?? Array.Empty<Guid>();

            var toAssign = await _context.Services
                .Where(s => selected.Contains(s.ID))
                .ToListAsync();
            foreach (var service in toAssign)
            {
                service.EmployeeID = employee.ID;
            }

            var toRelease = await _context.Services
                .Where(s => s.EmployeeID == employee.ID && !selected.Contains(s.ID))
                .ToListAsync();
            foreach (var service in toRelease)
            {
                service.EmployeeID = null;
            }
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Employees
                .Include(e => e.Services)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Services)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateServiceListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Salary,ID,Name,Address,DateOfBirth,PhotoFile,RemovePhoto")] Employee employee,
            Guid[] selectedServices)
        {
            var photoError = PhotoUpload.Validate(employee.PhotoFile);
            if (photoError != null)
            {
                ModelState.AddModelError(nameof(Employee.PhotoFile), photoError);
            }

            if (ModelState.IsValid)
            {
                employee.ID = Guid.NewGuid();
                await PhotoUpload.ApplyAsync(employee, employee.PhotoFile, employee.RemovePhoto);

                _context.Add(employee);
                await AssignServicesAsync(employee, selectedServices);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateServiceListAsync(selectedServices);
            return View(employee);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Services)
                .FirstOrDefaultAsync(e => e.ID == id);
            if (employee == null)
            {
                return NotFound();
            }

            await PopulateServiceListAsync(employee.Services.Select(s => s.ID));
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            [Bind("Salary,ID,Name,Address,DateOfBirth,PhotoFile,RemovePhoto")] Employee employee,
            Guid[] selectedServices)
        {
            if (id != employee.ID)
            {
                return NotFound();
            }

            var photoError = PhotoUpload.Validate(employee.PhotoFile);
            if (photoError != null)
            {
                ModelState.AddModelError(nameof(Employee.PhotoFile), photoError);
            }

            if (ModelState.IsValid)
            {
                var employeeToUpdate = await _context.Employees
                    .Include(e => e.Services)
                    .FirstOrDefaultAsync(e => e.ID == id);
                if (employeeToUpdate == null)
                {
                    return NotFound();
                }

                employeeToUpdate.Name = employee.Name;
                employeeToUpdate.Address = employee.Address;
                employeeToUpdate.DateOfBirth = employee.DateOfBirth;
                employeeToUpdate.Salary = employee.Salary;
                await PhotoUpload.ApplyAsync(employeeToUpdate, employee.PhotoFile, employee.RemovePhoto);
                await AssignServicesAsync(employeeToUpdate, selectedServices);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.ID))
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
            return View(employee);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Services)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var employee = await _context.Employees
                .Include(e => e.Services)
                .FirstOrDefaultAsync(e => e.ID == id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Serves an employee's stored photo so views can render it in an img tag.</summary>
        public async Task<IActionResult> Photo(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee?.Photo == null || employee.Photo.Length == 0)
            {
                return NotFound();
            }

            return File(employee.Photo, employee.PhotoContentType ?? "application/octet-stream");
        }

        private bool EmployeeExists(Guid id)
        {
            return _context.Employees.Any(e => e.ID == id);
        }
    }
}
