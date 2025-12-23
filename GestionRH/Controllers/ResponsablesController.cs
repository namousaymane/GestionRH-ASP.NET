using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionRH.Data;
using GestionRH.Models;
using GestionRH.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace GestionRH.Controllers
{
    [Authorize]
    public class ResponsablesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly NotificationService _notificationService;

        public ResponsablesController(ApplicationDbContext context, UserManager<Utilisateur> userManager, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // GET: Responsables
        public async Task<IActionResult> Index()
        {
            // Seuls les administrateurs RH peuvent voir la liste des responsables
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            var responsables = await _context.Responsables.ToListAsync();
            return View(responsables);
        }

        // GET: Responsables/Details/5
        public async Task<IActionResult> Details(string id)
        {
            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var responsable = await _context.Responsables
                .FirstOrDefaultAsync(m => m.Id == id);

            if (responsable == null)
            {
                return NotFound();
            }

            // Compter les employés sous sa responsabilité
            ViewBag.NombreEmployes = await _context.Employes
                .CountAsync(e => e.ManagerId == id);

            return View(responsable);
        }

        // GET: Responsables/Create
        public IActionResult Create()
        {
            // Seuls les administrateurs RH peuvent créer des responsables
            return View();
        }

        // POST: Responsables/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Prenom,Email")] Responsable responsable, string password)
        {
            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            // Configuration de base
            responsable.UserName = responsable.Email;
            responsable.Role = "Responsable";

            // Générer un mot de passe par défaut si non fourni
            if (string.IsNullOrEmpty(password))
            {
                password = "Manager123!";
            }

            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(responsable, password);
                
                if (result.Succeeded)
                {
                    // Notifier le responsable créé
                    await _notificationService.CreerNotificationAsync(
                        responsable.Id,
                        "Compte Responsable créé",
                        $"Votre compte responsable a été créé avec succès. Vous pouvez maintenant vous connecter avec votre email : {responsable.Email}",
                        "Responsable",
                        "/Employes/MonProfil"
                    );

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(responsable);
        }

        // GET: Responsables/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var responsable = await _context.Responsables.FindAsync(id);
            if (responsable == null)
            {
                return NotFound();
            }

            return View(responsable);
        }

        // POST: Responsables/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Nom,Prenom,Email")] Responsable responsable)
        {
            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            if (id != responsable.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingResponsable = await _context.Responsables.FindAsync(id);
                    if (existingResponsable == null)
                    {
                        return NotFound();
                    }

                    existingResponsable.Nom = responsable.Nom;
                    existingResponsable.Prenom = responsable.Prenom;
                    existingResponsable.Email = responsable.Email;
                    existingResponsable.UserName = responsable.Email;

                    _context.Update(existingResponsable);
                    await _context.SaveChangesAsync();

                    // Notifier le responsable des modifications
                    await _notificationService.CreerNotificationAsync(
                        existingResponsable.Id,
                        "Profil mis à jour",
                        "Vos informations ont été mises à jour par l'administrateur RH.",
                        "Responsable",
                        "/Employes/MonProfil"
                    );
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResponsableExists(responsable.Id))
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
            return View(responsable);
        }

        // GET: Responsables/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var responsable = await _context.Responsables
                .FirstOrDefaultAsync(m => m.Id == id);

            if (responsable == null)
            {
                return NotFound();
            }

            // Vérifier s'il y a des employés sous sa responsabilité
            var nombreEmployes = await _context.Employes
                .CountAsync(e => e.ManagerId == id);
            ViewBag.NombreEmployes = nombreEmployes;

            return View(responsable);
        }

        // POST: Responsables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            var responsable = await _context.Responsables.FindAsync(id);
            if (responsable != null)
            {
                // Retirer la référence ManagerId des employés avant de supprimer
                var employesAvecManager = await _context.Employes
                    .Where(e => e.ManagerId == id)
                    .ToListAsync();
                
                foreach (var employe in employesAvecManager)
                {
                    employe.ManagerId = null;
                    // Notifier les employés
                    await _notificationService.CreerNotificationAsync(
                        employe.Id,
                        "Manager supprimé",
                        $"Votre manager {responsable.NomComplet} n'est plus votre responsable.",
                        "Responsable",
                        null
                    );
                }

                // Notifier le responsable
                await _notificationService.CreerNotificationAsync(
                    responsable.Id,
                    "Compte supprimé",
                    "Votre compte responsable a été supprimé du système.",
                    "Responsable",
                    null
                );

                _context.Responsables.Remove(responsable);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ResponsableExists(string id)
        {
            return _context.Responsables.Any(e => e.Id == id);
        }
    }
}














