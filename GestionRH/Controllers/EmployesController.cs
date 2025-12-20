using GestionRH.Data;
using GestionRH.Models;
using GestionRH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GestionRH.Controllers
{
    [Authorize]
    public class EmployesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly NotificationService _notificationService;

        public EmployesController(ApplicationDbContext context, UserManager<Utilisateur> userManager, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // =========================================================
        // MÉTHODE PRIVÉE POUR VÉRIFIER L'ACCÈS
        // =========================================================
        private async Task<bool> EstAdminRH()
        {
            var user = await _userManager.GetUserAsync(User);
            return user != null && user.Role == "AdministrateurRH";
        }

        // GET: Employes
        public async Task<IActionResult> Index(string searchString, string departementFilter, string managerFilter)
        {
            // Seuls les Administrateurs RH et Responsables peuvent voir la liste des employés
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.Role != "AdministrateurRH" && user.Role != "Responsable")
            {
                return Forbid();
            }

            IQueryable<Employe> employesQuery = _context.Employes.Include(e => e.Departement);

            // Si c'est un Responsable, il ne voit que ses employés (ceux dont il est le manager)
            if (user.Role == "Responsable")
            {
                employesQuery = employesQuery.Where(e => e.ManagerId == user.Id);
            }
            // L'AdministrateurRH voit tous les employés

            // FILTRES DE RECHERCHE
            if (!string.IsNullOrEmpty(searchString))
            {
                employesQuery = employesQuery.Where(e => 
                    e.Nom.Contains(searchString) || 
                    e.Prenom.Contains(searchString) ||
                    (e.Email != null && e.Email.Contains(searchString)) ||
                    e.Poste.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(departementFilter) && int.TryParse(departementFilter, out int deptId))
            {
                employesQuery = employesQuery.Where(e => e.DepartementId == deptId);
            }

            if (!string.IsNullOrEmpty(managerFilter))
            {
                employesQuery = employesQuery.Where(e => e.ManagerId == managerFilter);
            }

            // Préparer les listes pour les dropdowns
            var departements = await _context.Departements.ToListAsync();
            var responsables = await _context.Responsables.ToListAsync();

            ViewBag.Departements = departements;
            ViewBag.Responsables = responsables;
            ViewBag.SearchString = searchString;
            ViewBag.DepartementFilter = departementFilter;
            ViewBag.ManagerFilter = managerFilter;

            var employes = await employesQuery.ToListAsync();
            return View(employes);
        }

        // GET: Employes/Create
        public async Task<IActionResult> Create()
        {
            // Seuls les Administrateurs RH peuvent créer des employés
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            // Charger les listes pour les dropdowns
            ViewBag.Responsables = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Responsables.ToListAsync(),
                "Id",
                "NomComplet"
            );

            ViewBag.Departements = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Departements.ToListAsync(),
                "DepartementId",
                "Nom"
            );

            return View();
        }

        // POST: Employes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Prenom,Email,Poste,Salaire,DateEmbauche,ManagerId,DepartementId")] Employe employe, string password)
        {
            // Vérifier les autorisations
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            // On force le rôle et le type par défaut
            employe.UserName = employe.Email; // Le username est l'email
            employe.Role = "Employe";
            employe.SecurityStamp = Guid.NewGuid().ToString();

            // Générer un mot de passe par défaut si non fourni
            if (string.IsNullOrEmpty(password))
            {
                password = "MotDePasse123!"; // Mot de passe par défaut
            }

            // On retire les erreurs de validation pour les champs qu'on gère nous-mêmes
            ModelState.Remove("ManagerId");
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (ModelState.IsValid)
            {
                // Créer l'utilisateur avec Identity
                var result = await _userManager.CreateAsync(employe, password);
                
                if (result.Succeeded)
                {
                    // Envoyer une notification à l'employé créé
                    await _notificationService.CreerNotificationAsync(
                        employe.Id,
                        "Bienvenue dans GestionRH",
                        $"Votre compte a été créé avec succès. Vous pouvez maintenant vous connecter avec votre email : {employe.Email}",
                        "Employe",
                        "/Employes/MonProfil"
                    );

                    // Notifier le manager si assigné
                    if (!string.IsNullOrEmpty(employe.ManagerId))
                    {
                        await _notificationService.CreerNotificationAsync(
                            employe.ManagerId,
                            "Nouvel employé assigné",
                            $"{employe.NomComplet} a été ajouté à votre équipe.",
                            "Employe",
                            $"/Employes/Details/{employe.Id}"
                        );
                    }

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Ajouter les erreurs de création d'utilisateur au ModelState
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // Recharger les listes en cas d'erreur
            ViewBag.Responsables = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Responsables.ToListAsync(),
                "Id",
                "NomComplet",
                employe.ManagerId
            );

            ViewBag.Departements = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Departements.ToListAsync(),
                "DepartementId",
                "Nom",
                employe.DepartementId
            );

            return View(employe);
        }

        // GET: Employes/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var employe = await _context.Employes
                .Include(e => e.Conges)
                .Include(e => e.Paies)
                .Include(e => e.Departement)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (!string.IsNullOrEmpty(employe?.ManagerId))
            {
                ViewBag.Manager = await _context.Users.FindAsync(employe.ManagerId);
            }

            if (employe == null)
            {
                return NotFound();
            }

            // Vérifier les autorisations
            if (user.Role == "Responsable" && employe.ManagerId != user.Id)
            {
                return Forbid();
            }
            else if (user.Role != "AdministrateurRH" && user.Role != "Responsable")
            {
                return Forbid();
            }

            return View(employe);
        }

        // GET: Employes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            // Seuls les Administrateurs RH peuvent modifier des employés
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            if (id == null) return NotFound();

            var employe = await _context.Employes.FindAsync(id);
            if (employe == null) return NotFound();

            // Charger la liste des responsables pour le dropdown ManagerId
            ViewBag.Responsables = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Responsables.ToListAsync(),
                "Id",
                "NomComplet",
                employe.ManagerId
            );

            // Charger la liste des départements pour le dropdown DepartementId
            ViewBag.Departements = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Departements.ToListAsync(),
                "DepartementId",
                "Nom",
                employe.DepartementId
            );

            return View(employe);
        }

        // POST: Employes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Nom,Prenom,Email,Poste,Salaire,ManagerId,DepartementId")] Employe employe)
        {
            // Vérifier les autorisations
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            if (id != employe.Id) return NotFound();

            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (ModelState.IsValid)
            {
                try
                {
                    // On garde les champs Identity inchangés
                    var existingEmploye = await _context.Employes.FindAsync(id);
                    if (existingEmploye == null)
                    {
                        return NotFound();
                    }

                    existingEmploye.Nom = employe.Nom;
                    existingEmploye.Prenom = employe.Prenom;
                    existingEmploye.Email = employe.Email;
                    existingEmploye.UserName = employe.Email; // Synchroniser avec Email
                    existingEmploye.Poste = employe.Poste;
                    existingEmploye.Salaire = employe.Salaire;
                    // Vérifier si le manager a changé
                    var ancienManagerId = existingEmploye.ManagerId;
                    existingEmploye.ManagerId = employe.ManagerId;
                    existingEmploye.DepartementId = employe.DepartementId;

                    _context.Update(existingEmploye);
                    await _context.SaveChangesAsync();

                    // Notifier l'employé des modifications
                    await _notificationService.CreerNotificationAsync(
                        existingEmploye.Id,
                        "Profil mis à jour",
                        "Vos informations ont été mises à jour par l'administrateur RH.",
                        "Employe",
                        "/Employes/MonProfil"
                    );

                    // Notifier le nouveau manager si changé
                    if (!string.IsNullOrEmpty(employe.ManagerId) && employe.ManagerId != ancienManagerId)
                    {
                        await _notificationService.CreerNotificationAsync(
                            employe.ManagerId,
                            "Nouvel employé assigné",
                            $"{existingEmploye.NomComplet} a été assigné à votre équipe.",
                            "Employe",
                            $"/Employes/Details/{existingEmploye.Id}"
                        );
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeExists(employe.Id))
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

            // Recharger les listes en cas d'erreur
            ViewBag.Responsables = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Responsables.ToListAsync(),
                "Id",
                "NomComplet",
                employe.ManagerId
            );

            ViewBag.Departements = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Departements.ToListAsync(),
                "DepartementId",
                "Nom",
                employe.DepartementId
            );

            return View(employe);
        }

        // GET: Employes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            // Seuls les Administrateurs RH peuvent supprimer des employés
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            if (id == null) return NotFound();

            var employe = await _context.Employes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employe == null) return NotFound();

            return View(employe);
        }

        // POST: Employes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // Vérifier les autorisations
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            var employe = await _context.Employes.FindAsync(id);
            if (employe != null)
            {
                // Notifier l'employé avant suppression
                await _notificationService.CreerNotificationAsync(
                    employe.Id,
                    "Compte supprimé",
                    "Votre compte a été supprimé du système.",
                    "Employe",
                    null
                );

                _context.Employes.Remove(employe);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Employes/MonProfil
        public async Task<IActionResult> MonProfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Identity/Account/Login");

            // Récupérer l'utilisateur selon son type
            Utilisateur? utilisateur = null;

            if (user.Role == "Employe")
            {
                utilisateur = await _context.Employes
                    .Include(e => e.Departement)
                    .FirstOrDefaultAsync(e => e.Id == user.Id);
            }
            else if (user.Role == "Responsable")
            {
                utilisateur = await _context.Responsables
                    .FirstOrDefaultAsync(r => r.Id == user.Id);
            }
            else if (user.Role == "AdministrateurRH")
            {
                utilisateur = await _context.AdministrateursRH
                    .FirstOrDefaultAsync(a => a.Id == user.Id);
            }

            if (utilisateur == null)
            {
                // Si l'utilisateur n'est trouvé dans aucune table, utiliser l'utilisateur de base
                utilisateur = user;
            }

            return View(utilisateur);
        }

        private bool EmployeExists(string id)
        {
            return _context.Employes.Any(e => e.Id == id);
        }
    }
}
