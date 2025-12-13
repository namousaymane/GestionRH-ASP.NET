using GestionRH.Data;
using GestionRH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionRH.Controllers
{
    [Authorize]
    public class ContratsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;

        public ContratsController(ApplicationDbContext context, UserManager<Utilisateur> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<bool> EstAdminRH()
        {
            var user = await _userManager.GetUserAsync(User);
            return user != null && user.Role == "AdministrateurRH";
        }

        // GET: Contrats
        public async Task<IActionResult> Index(string searchString, string typeFilter)
        {
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            IQueryable<Contrat> contratsQuery = _context.Contrats.Include(c => c.Employe);

            if (!string.IsNullOrEmpty(searchString))
            {
                contratsQuery = contratsQuery.Where(c => 
                    c.Employe.Nom.Contains(searchString) || 
                    c.Employe.Prenom.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(typeFilter))
            {
                contratsQuery = contratsQuery.Where(c => c.TypeContrat == typeFilter);
            }

            ViewBag.SearchString = searchString;
            ViewBag.TypeFilter = typeFilter;
            ViewBag.Types = new[] { "CDI", "CDD", "Stage", "Freelance" };

            var contrats = await contratsQuery.OrderByDescending(c => c.DateDebut).ToListAsync();
            return View(contrats);
        }

        // GET: Contrats/Create
        public async Task<IActionResult> Create()
        {
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            ViewBag.Employes = await _context.Employes.ToListAsync();
            ViewBag.Types = new[] { "CDI", "CDD", "Stage", "Freelance" };
            return View();
        }

        // POST: Contrats/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeId,TypeContrat,DateDebut,DateFin,SalaireBrut,Description,EstActif")] Contrat contrat)
        {
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                _context.Add(contrat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Employes = await _context.Employes.ToListAsync();
            ViewBag.Types = new[] { "CDI", "CDD", "Stage", "Freelance" };
            return View(contrat);
        }

        // GET: Contrats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var contrat = await _context.Contrats
                .Include(c => c.Employe)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contrat == null)
            {
                return NotFound();
            }

            return View(contrat);
        }

        // GET: Contrats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var contrat = await _context.Contrats.FindAsync(id);
            if (contrat == null)
            {
                return NotFound();
            }

            ViewBag.Employes = await _context.Employes.ToListAsync();
            ViewBag.Types = new[] { "CDI", "CDD", "Stage", "Freelance" };
            return View(contrat);
        }

        // POST: Contrats/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeId,TypeContrat,DateDebut,DateFin,SalaireBrut,Description,EstActif")] Contrat contrat)
        {
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            if (id != contrat.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contrat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContratExists(contrat.Id))
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

            ViewBag.Employes = await _context.Employes.ToListAsync();
            ViewBag.Types = new[] { "CDI", "CDD", "Stage", "Freelance" };
            return View(contrat);
        }

        // GET: Contrats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var contrat = await _context.Contrats
                .Include(c => c.Employe)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contrat == null)
            {
                return NotFound();
            }

            return View(contrat);
        }

        // POST: Contrats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            var contrat = await _context.Contrats.FindAsync(id);
            if (contrat != null)
            {
                _context.Contrats.Remove(contrat);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ContratExists(int id)
        {
            return _context.Contrats.Any(e => e.Id == id);
        }
    }
}

