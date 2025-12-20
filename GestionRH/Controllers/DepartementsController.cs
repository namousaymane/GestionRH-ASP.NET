using GestionRH.Data;
using GestionRH.Models;
using GestionRH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GestionRH.Controllers
{
    [Authorize]
    public class DepartementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly NotificationService _notificationService;

        public DepartementsController(ApplicationDbContext context, UserManager<Utilisateur> userManager, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // Vérifier si l'utilisateur est AdministrateurRH
        private async Task<bool> EstAdminRH()
        {
            var user = await _userManager.GetUserAsync(User);
            return user != null && user.Role == "AdministrateurRH";
        }

        // Vérifier si l'utilisateur est AdministrateurRH ou Responsable
        private async Task<bool> EstAdminRHouResponsable()
        {
            var user = await _userManager.GetUserAsync(User);
            return user != null && (user.Role == "AdministrateurRH" || user.Role == "Responsable");
        }

        // GET: Departements
        public async Task<IActionResult> Index()
        {
            // Les Administrateurs RH et les Responsables peuvent voir la liste des départements
            if (!await EstAdminRHouResponsable())
            {
                return Forbid();
            }

            var departements = await _context.Departements
                .Include(d => d.Chef)
                .Include(d => d.Employes)
                .ToListAsync();

            return View(departements);
        }

        // GET: Departements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!await EstAdminRHouResponsable())
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var departement = await _context.Departements
                .Include(d => d.Chef)
                .Include(d => d.Employes)
                .FirstOrDefaultAsync(m => m.DepartementId == id);

            if (departement == null)
            {
                return NotFound();
            }

            return View(departement);
        }

        // GET: Departements/Create
        public async Task<IActionResult> Create()
        {
            // Seuls les Administrateurs RH peuvent créer des départements
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            // Charger la liste des employés pour le dropdown Chef
            // Charger la liste des employés et responsables pour le dropdown Chef
            var eligibleChefs = await _context.Users
                .Where(u => u.Role == "Employe" || u.Role == "Responsable")
                .ToListAsync();

            ViewBag.Employes = new SelectList(
                eligibleChefs,
                "Id",
                "NomComplet"
            );

            return View();
        }

        // POST: Departements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,ChefId")] Departement departement)
        {
            // Seuls les Administrateurs RH peuvent créer des départements
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            // Convertir une chaîne vide en null pour ChefId
            if (string.IsNullOrWhiteSpace(departement.ChefId))
            {
                departement.ChefId = null;
            }

            // Vérifier si le ChefId existe si fourni
            if (!string.IsNullOrEmpty(departement.ChefId))
            {
                var chefExists = await _context.Users.AnyAsync(e => e.Id == departement.ChefId);
                if (!chefExists)
                {
                    ModelState.AddModelError("ChefId", "L'utilisateur sélectionné n'existe pas.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(departement);
                    await _context.SaveChangesAsync();

                    // Notifier le chef du département si assigné
                    if (!string.IsNullOrEmpty(departement.ChefId))
                    {
                        await _notificationService.CreerNotificationAsync(
                            departement.ChefId,
                            "Nouveau département",
                            $"Vous avez été nommé chef du département {departement.Nom}.",
                            "Departement",
                            $"/Departements/Details/{departement.DepartementId}"
                        );
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Une erreur est survenue lors de la création du département. " + ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Une erreur inattendue est survenue. " + ex.Message);
                }
            }

            // Recharger la liste des employés en cas d'erreur
            // Recharger la liste des utilisateurs en cas d'erreur
            var eligibleChefs = await _context.Users
                .Where(u => u.Role == "Employe" || u.Role == "Responsable")
                .ToListAsync();

            ViewBag.Employes = new SelectList(
                eligibleChefs,
                "Id",
                "NomComplet",
                departement.ChefId
            );

            return View(departement);
        }

        // GET: Departements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Seuls les Administrateurs RH peuvent modifier des départements
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var departement = await _context.Departements.FindAsync(id);
            if (departement == null)
            {
                return NotFound();
            }

            // Charger la liste des employés pour le dropdown Chef
            // Charger la liste des employés et responsables pour le dropdown Chef
            var eligibleChefs = await _context.Users
                .Where(u => u.Role == "Employe" || u.Role == "Responsable")
                .ToListAsync();

            ViewBag.Employes = new SelectList(
                eligibleChefs,
                "Id",
                "NomComplet",
                departement.ChefId
            );

            return View(departement);
        }

        // POST: Departements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DepartementId,Nom,ChefId")] Departement departement)
        {
            // Seuls les Administrateurs RH peuvent modifier des départements
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            if (id != departement.DepartementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingDepartement = await _context.Departements.FindAsync(id);
                    if (existingDepartement == null) return NotFound();

                    var ancienChefId = existingDepartement.ChefId;

                    // Mise à jour des propriétés
                    existingDepartement.Nom = departement.Nom;
                    existingDepartement.ChefId = departement.ChefId;

                    // _context.Update(departement); // CAUSE DU CONFLIT
                    await _context.SaveChangesAsync();

                    // Notifier le nouveau chef si changé
                    if (!string.IsNullOrEmpty(existingDepartement.ChefId) && existingDepartement.ChefId != ancienChefId)
                    {
                        await _notificationService.CreerNotificationAsync(
                            existingDepartement.ChefId,
                            "Nomination chef de département",
                            $"Vous avez été nommé chef du département {existingDepartement.Nom}.",
                            "Departement",
                            $"/Departements/Details/{existingDepartement.DepartementId}"
                        );
                    }

                    // Notifier les employés du département
                    var employes = await _context.Employes
                        .Where(e => e.DepartementId == departement.DepartementId)
                        .ToListAsync();
                    
                    foreach (var employe in employes)
                    {
                        await _notificationService.CreerNotificationAsync(
                            employe.Id,
                            "Département mis à jour",
                            $"Le département {departement.Nom} a été mis à jour.",
                            "Departement",
                            $"/Departements/Details/{departement.DepartementId}"
                        );
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartementExists(departement.DepartementId))
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

            // Recharger la liste des utilisateurs en cas d'erreur
            var eligibleChefs = await _context.Users
                .Where(u => u.Role == "Employe" || u.Role == "Responsable")
                .ToListAsync();

            ViewBag.Employes = new SelectList(
                eligibleChefs,
                "Id",
                "NomComplet",
                departement.ChefId
            );

            return View(departement);
        }

        // GET: Departements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Seuls les Administrateurs RH peuvent supprimer des départements
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var departement = await _context.Departements
                .Include(d => d.Chef)
                .Include(d => d.Employes)
                .FirstOrDefaultAsync(m => m.DepartementId == id);

            if (departement == null)
            {
                return NotFound();
            }

            return View(departement);
        }

        // POST: Departements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Seuls les Administrateurs RH peuvent supprimer des départements
            if (!await EstAdminRH())
            {
                return Forbid();
            }

            var departement = await _context.Departements
                .Include(d => d.Employes)
                .FirstOrDefaultAsync(d => d.DepartementId == id);
            
            if (departement != null)
            {
                // Notifier les employés du département
                foreach (var employe in departement.Employes)
                {
                    await _notificationService.CreerNotificationAsync(
                        employe.Id,
                        "Département supprimé",
                        $"Le département {departement.Nom} auquel vous apparteniez a été supprimé.",
                        "Departement",
                        null
                    );
                }

                _context.Departements.Remove(departement);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DepartementExists(int id)
        {
            return _context.Departements.Any(e => e.DepartementId == id);
        }
    }
}



