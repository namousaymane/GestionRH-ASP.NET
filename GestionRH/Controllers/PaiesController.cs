using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionRH.Data;
using GestionRH.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace GestionRH.Controllers
{
    [Authorize]
    public class PaiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;

        public PaiesController(ApplicationDbContext context, UserManager<Utilisateur> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Paies
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            IQueryable<Paie> paiesQuery = _context.Paies.Include(p => p.Employe);

            // LOGIQUE DE FILTRAGE SELON LE RÔLE
            if (user.Role == "AdministrateurRH")
            {
                // L'admin voit TOUT
            }
            else if (user.Role == "Responsable")
            {
                // Le manager voit les paies de son équipe
                paiesQuery = paiesQuery.Where(p => p.Employe.ManagerId == user.Id);
            }
            else // Employé simple
            {
                // Ne voit que SES paies
                paiesQuery = paiesQuery.Where(p => p.EmployeId == user.Id);
            }

            var paies = await paiesQuery.OrderByDescending(p => p.DateEmission).ToListAsync();
            return View(paies);
        }

        // GET: Paies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paie = await _context.Paies
                .Include(p => p.Employe)
                .FirstOrDefaultAsync(m => m.IdPaie == id);

            if (paie == null)
            {
                return NotFound();
            }

            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.Role != "AdministrateurRH" && 
                user.Role != "Responsable" && 
                paie.EmployeId != user.Id)
            {
                return Forbid();
            }

            return View(paie);
        }

        // GET: Paies/Create
        public async Task<IActionResult> Create()
        {
            // Seuls les administrateurs RH peuvent créer des paies
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            ViewBag.Employes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Employes.ToListAsync(), 
                "Id", 
                "NomComplet"
            );
            return View();
        }

        // POST: Paies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Montant,Mois,DateEmission,EmployeId")] Paie paie)
        {
            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            ModelState.Remove("Employe");

            if (ModelState.IsValid)
            {
                _context.Add(paie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Employes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Employes.ToListAsync(), 
                "Id", 
                "NomComplet",
                paie.EmployeId
            );
            return View(paie);
        }

        // GET: Paies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Seuls les administrateurs RH peuvent modifier des paies
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            var paie = await _context.Paies.FindAsync(id);
            if (paie == null)
            {
                return NotFound();
            }

            ViewBag.Employes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Employes.ToListAsync(), 
                "Id", 
                "NomComplet",
                paie.EmployeId
            );
            return View(paie);
        }

        // POST: Paies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPaie,Montant,Mois,DateEmission,EmployeId")] Paie paie)
        {
            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            if (id != paie.IdPaie)
            {
                return NotFound();
            }

            ModelState.Remove("Employe");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaieExists(paie.IdPaie))
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

            ViewBag.Employes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Employes.ToListAsync(), 
                "Id", 
                "NomComplet",
                paie.EmployeId
            );
            return View(paie);
        }

        // GET: Paies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Seuls les administrateurs RH peuvent supprimer des paies
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            var paie = await _context.Paies
                .Include(p => p.Employe)
                .FirstOrDefaultAsync(m => m.IdPaie == id);

            if (paie == null)
            {
                return NotFound();
            }

            return View(paie);
        }

        // POST: Paies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            var paie = await _context.Paies.FindAsync(id);
            if (paie != null)
            {
                _context.Paies.Remove(paie);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PaieExists(int id)
        {
            return _context.Paies.Any(e => e.IdPaie == id);
        }
    }
}

