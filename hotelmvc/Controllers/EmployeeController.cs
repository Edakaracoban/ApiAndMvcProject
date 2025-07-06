using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using hotelmvc.Models;
using System.Threading.Tasks;
using System.Linq;

namespace hotelmvc.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly MvcDbContext _context;

        public EmployeeController(MvcDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.Include(e => e.Branch).ToListAsync();
            return View(employees);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .Include(e => e.Branch)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        public IActionResult Create()
        {
            ViewBag.BranchList = new SelectList(_context.Branches, "BranchId", "BranchName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.BranchList = new SelectList(_context.Branches, "BranchId", "BranchName", employee.BranchId);
            return View(employee);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            ViewBag.BranchList = new SelectList(_context.Branches, "BranchId", "BranchName", employee.BranchId);
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.EmployeeId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.BranchList = new SelectList(_context.Branches, "BranchId", "BranchName", employee.BranchId);
            return View(employee);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}
