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
    public class EmployesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;

        public EmployesController(ApplicationDbContext context, UserManager<Utilisateur> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Employes
        public async Task<IActionResult> Index()
        {
            // Seuls les Administrateurs RH et Responsables peuvent voir la liste des employés
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.Role != "AdministrateurRH" && user.Role != "Responsable")
            {
                return Forbid();
            }

            IQueryable<Employe> employesQuery = _context.Employes;

            // Si c'est un Responsable, il ne voit que ses employés (ceux dont il est le manager)
            if (user.Role == "Responsable")
            {
                employesQuery = employesQuery.Where(e => e.ManagerId == user.Id);
            }
            // L'AdministrateurRH voit tous les employés

            var employes = await employesQuery.ToListAsync();
            return View(employes);
        }

        // GET: Employes/Create
        public async Task<IActionResult> Create()
        {
            // Seuls les Administrateurs RH peuvent créer des employés
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }
            return View();
        }

        // POST: Employes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Prenom,Email,Poste,Salaire,DateEmbauche")] Employe employe, string password)
        {
            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            // On force le rôle et le type par défaut
            employe.UserName = employe.Email; // Le username est l'email
            employe.Role = "Employe";

            // Générer un mot de passe par défaut si non fourni
            if (string.IsNullOrEmpty(password))
            {
                password = "MotDePasse123!"; // Mot de passe par défaut
            }

            if (ModelState.IsValid)
            {
                // Créer l'utilisateur avec Identity
                var result = await _userManager.CreateAsync(employe, password);
                
                if (result.Succeeded)
                {
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
                .FirstOrDefaultAsync(m => m.Id == id);

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
            if (id == null)
            {
                return NotFound();
            }

            // Seuls les Administrateurs RH peuvent modifier des employés
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            var employe = await _context.Employes.FindAsync(id);
            if (employe == null)
            {
                return NotFound();
            }

            // Charger la liste des responsables pour le dropdown ManagerId
            ViewBag.Responsables = await _context.Responsables.ToListAsync();
            return View(employe);
        }

        // POST: Employes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Nom,Prenom,Email,Poste,Salaire,ManagerId")] Employe employe)
        {
            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            if (id != employe.Id)
            {
                return NotFound();
            }

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
                    existingEmploye.ManagerId = employe.ManagerId;

                    _context.Update(existingEmploye);
                    await _context.SaveChangesAsync();
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

            ViewBag.Responsables = await _context.Responsables.ToListAsync();
            return View(employe);
        }

        // GET: Employes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Seuls les Administrateurs RH peuvent supprimer des employés
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            var employe = await _context.Employes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employe == null)
            {
                return NotFound();
            }

            return View(employe);
        }

        // POST: Employes/Delete/5
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

            var employe = await _context.Employes.FindAsync(id);
            if (employe != null)
            {
                _context.Employes.Remove(employe);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EmployeExists(string id)
        {
            return _context.Employes.Any(e => e.Id == id);
        }
    }
}