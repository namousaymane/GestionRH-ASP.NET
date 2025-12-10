using GestionRH.Data;
using GestionRH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GestionRH.Controllers
{
    public class EmployesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;

        public EmployesController(ApplicationDbContext context, UserManager<Utilisateur> userManager)
        {
            _context = context;
            _userManager = userManager;
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
        public async Task<IActionResult> Index()
        {
            // SÉCURITÉ
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            var employes = await _context.Employes.ToListAsync();
            return View(employes);
        }

        // GET: Employes/Create
        public async Task<IActionResult> Create()
        {
            // SÉCURITÉ
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            return View();
        }

        // POST: Employes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Prenom,Email,Poste,Salaire,DateEmbauche")] Employe employe)
        {
            // SÉCURITÉ
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            employe.UserName = employe.Email;
            employe.Role = "Employe";
            employe.SecurityStamp = Guid.NewGuid().ToString();

            // On retire les erreurs de validation pour les champs qu'on gère nous-mêmes
            ModelState.Remove("ManagerId");
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (ModelState.IsValid)
            {
                _context.Add(employe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employe);
        }

        // GET: Employes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            // SÉCURITÉ
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            if (id == null) return NotFound();

            var employe = await _context.Employes.FindAsync(id);
            if (employe == null) return NotFound();

            return View(employe);
        }

        // POST: Employes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Nom,Prenom,Email,Poste,Salaire,ManagerId")] Employe employe)
        {
            // SÉCURITÉ
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            if (id != employe.Id) return NotFound();

            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (ModelState.IsValid)
            {
                try
                {
                    var employeOriginal = await _context.Employes.FindAsync(id);
                    if (employeOriginal == null) return NotFound();

                    // Mise à jour des champs
                    employeOriginal.Nom = employe.Nom;
                    employeOriginal.Prenom = employe.Prenom;
                    employeOriginal.Poste = employe.Poste;
                    employeOriginal.Salaire = employe.Salaire;
                    employeOriginal.ManagerId = employe.ManagerId;
                    employeOriginal.Email = employe.Email;
                    employeOriginal.UserName = employe.Email;

                    _context.Update(employeOriginal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Employes.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employe);
        }

        // GET: Employes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            // SÉCURITÉ
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            if (id == null) return NotFound();

            var employe = await _context.Employes.FindAsync(id);
            if (employe == null) return NotFound();

            return View(employe);
        }

        // POST: Employes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // SÉCURITÉ
            if (!await EstAdminRH()) return Redirect("/Identity/Account/Login");

            var employe = await _context.Employes.FindAsync(id);
            if (employe != null)
            {
                _context.Employes.Remove(employe);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Employes/MonProfil
        [AllowAnonymous] // Ou gérer la sécurité pour que l'employé connecté puisse y accéder
        public async Task<IActionResult> MonProfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/Identity/Account/Login");

            // On recharge l'employé avec son département
            var employe = await _context.Employes
                .Include(e => e.Departement)
                .FirstOrDefaultAsync(e => e.Id == user.Id);

            return View(employe);
        }
    }
}