using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionRH.Data;
using GestionRH.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using GestionRH.Services;
using System.Linq;
using System.Threading.Tasks;

namespace GestionRH.Controllers
{
    [Authorize]
    public class PaiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly PdfService _pdfService;
        private readonly NotificationService _notificationService;

        public PaiesController(ApplicationDbContext context, UserManager<Utilisateur> userManager, PdfService pdfService, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _pdfService = pdfService;
            _notificationService = notificationService;
        }

        // GET: Paies
        public async Task<IActionResult> Index(string searchString, string moisFilter, string employeFilter)
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

            // FILTRES DE RECHERCHE
            if (!string.IsNullOrEmpty(searchString))
            {
                paiesQuery = paiesQuery.Where(p => 
                    p.Employe.Nom.Contains(searchString) || 
                    p.Employe.Prenom.Contains(searchString) ||
                    p.Mois.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(moisFilter))
            {
                paiesQuery = paiesQuery.Where(p => p.Mois.Contains(moisFilter));
            }

            if (!string.IsNullOrEmpty(employeFilter) && user.Role == "AdministrateurRH")
            {
                paiesQuery = paiesQuery.Where(p => p.EmployeId == employeFilter);
            }

            // Préparer les listes pour les dropdowns
            var employes = await _context.Employes.ToListAsync();
            var mois = await _context.Paies.Select(p => p.Mois).Distinct().OrderByDescending(m => m).ToListAsync();

            ViewBag.Employes = employes;
            ViewBag.Mois = mois;
            ViewBag.SearchString = searchString;
            ViewBag.MoisFilter = moisFilter;
            ViewBag.EmployeFilter = employeFilter;

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
                .Include(p => p.LignesPaie)
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

            // Trier les lignes par ordre
            paie.LignesPaie = paie.LignesPaie.OrderBy(l => l.Ordre).ToList();

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

            var employes = await _context.Employes.ToListAsync();
            
            // Vérifier s'il y a des employés
            if (!employes.Any())
            {
                ViewBag.Message = "Aucun employé disponible. Veuillez d'abord créer des employés.";
            }

            ViewBag.Employes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                employes, 
                "Id", 
                "NomComplet"
            );
            return View();
        }

        // POST: Paies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string EmployeId, string Mois, int Annee, decimal Primes, decimal Retenues)
        {
            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            // Vérifier qu'un employé est sélectionné
            if (string.IsNullOrEmpty(EmployeId))
            {
                ModelState.AddModelError("EmployeId", "Veuillez sélectionner un employé.");
            }

            // Récupérer l'employé pour calculer le montant
            var employe = await _context.Employes.FindAsync(EmployeId);
            if (employe == null)
            {
                ModelState.AddModelError("EmployeId", "L'employé sélectionné n'existe pas.");
            }

            if (ModelState.IsValid && employe != null)
            {
                // Combiner Mois et Année
                string moisComplet = $"{Mois} {Annee}";
                
                // Calculer les cotisations (CNSS, AMO, etc.)
                decimal salaireBrut = employe.Salaire + Primes;
                decimal cnss = salaireBrut * 0.0269m; // 2.69% pour CNSS
                decimal amo = salaireBrut * 0.0226m; // 2.26% pour AMO
                decimal totalCotisations = cnss + amo;
                decimal montantNet = salaireBrut - totalCotisations - Retenues;

                var paie = new Paie
                {
                    EmployeId = EmployeId,
                    Mois = moisComplet,
                    Montant = montantNet,
                    DateEmission = DateTime.Now
                };

                _context.Add(paie);
                await _context.SaveChangesAsync();

                // Créer les lignes de paie
                var lignesPaie = new List<LignePaie>
                {
                    new LignePaie { PaieId = paie.IdPaie, Libelle = "Salaire de base", Montant = employe.Salaire, Type = "Gain", Ordre = 1 },
                    new LignePaie { PaieId = paie.IdPaie, Libelle = "Primes / Bonus", Montant = Primes, Type = "Gain", Ordre = 2 },
                    new LignePaie { PaieId = paie.IdPaie, Libelle = "Salaire brut", Montant = salaireBrut, Type = "Gain", Ordre = 3 },
                    new LignePaie { PaieId = paie.IdPaie, Libelle = "CNSS (2.69%)", Montant = cnss, Type = "Retenue", Ordre = 4 },
                    new LignePaie { PaieId = paie.IdPaie, Libelle = "AMO (2.26%)", Montant = amo, Type = "Retenue", Ordre = 5 },
                    new LignePaie { PaieId = paie.IdPaie, Libelle = "Total cotisations", Montant = totalCotisations, Type = "Retenue", Ordre = 6 },
                    new LignePaie { PaieId = paie.IdPaie, Libelle = "Autres retenues", Montant = Retenues, Type = "Retenue", Ordre = 7 }
                };

                _context.LignesPaie.AddRange(lignesPaie);
                await _context.SaveChangesAsync();

                // Notifier l'employé
                await _notificationService.CreerNotificationAsync(
                    EmployeId,
                    "Nouveau bulletin de paie",
                    $"Votre bulletin de paie pour {moisComplet} est disponible.",
                    "Paie",
                    Url.Action("Details", "Paies", new { id = paie.IdPaie })
                );

                return RedirectToAction(nameof(Index));
            }

            // Recharger la liste des employés pour la vue
            ViewBag.Employes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Employes.ToListAsync(), 
                "Id", 
                "NomComplet",
                EmployeId
            );
            return View();
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

        // GET: Paies/DownloadPdf/5
        public async Task<IActionResult> DownloadPdf(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paie = await _context.Paies
                .Include(p => p.Employe)
                .ThenInclude(e => e.Departement)
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

            // Générer le PDF
            var pdfBytes = _pdfService.GenererBulletinPaie(paie);

            // Retourner le fichier PDF
            var fileName = $"Bulletin_Paie_{paie.Employe.NomComplet.Replace(" ", "_")}_{paie.Mois.Replace("/", "_")}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        private bool PaieExists(int id)
        {
            return _context.Paies.Any(e => e.IdPaie == id);
        }
    }
}

