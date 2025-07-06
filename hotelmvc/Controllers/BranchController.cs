using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hotelmvc.Models;
using System.Threading.Tasks;
using System.Linq;

namespace hotelmvc.Controllers
{
    public class BranchController : Controller
    {
        private readonly MvcDbContext _context;

        public BranchController(MvcDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var branches = await _context.Branches.ToListAsync();
            return View(branches);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.BranchId == id);
            if (branch == null) return NotFound();

            return View(branch);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Branch branch)
        {
            if (ModelState.IsValid)
            {
                _context.Add(branch);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(branch);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound();

            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Branch branch)
        {
            if (id != branch.BranchId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(branch);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BranchExists(branch.BranchId))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(branch);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound();

            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool BranchExists(int id)
        {
            return _context.Branches.Any(e => e.BranchId == id);
        }
    }
}
