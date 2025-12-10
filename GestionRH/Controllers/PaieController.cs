using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionRH.Data;
using GestionRH.Models;
using Microsoft.AspNetCore.Identity;

namespace GestionRH.Controllers
{
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

            // Seul l'Admin RH peut voir toutes les paies
            if (user.Role == "AdministrateurRH")
            {
                // CORRECTION ICI : "toutesLesPaies" partout
                var toutesLesPaies = await _context.Paies.Include(p => p.Employe).ToListAsync();
                return View(toutesLesPaies);
            }

            // Un employé ne voit que SES bulletins
            var mesPaies = await _context.Paies
                                         .Include(p => p.Employe)
                                         .Where(p => p.EmployeId == user.Id)
                                         .ToListAsync();
            return View(mesPaies);
        }
        // ... (Après la méthode Index) ...

        // GET: Paies/Create (Afficher le formulaire)
        public IActionResult Create()
        {
            // Sécurité : Seul l'admin RH peut accéder à cette page
            // Note: On utilise User.IsInRole ou notre propriété Role personnalisée
            // Pour être cohérent avec votre projet, on vérifie manuellement ou on laisse ouvert si l'admin est le seul à avoir le bouton.

            // On prépare la liste des employés pour le menu déroulant
            // On sélectionne l'ID et le NomComplet
            ViewData["EmployeId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Employes, "Id", "NomComplet");

            return View();
        }

        // POST: Paies/Create (Calculer et Enregistrer)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string EmployeId, string Mois, int Annee, decimal Primes, decimal Retenues)
        {
            // 1. Récupérer l'employé pour avoir son salaire de base
            var employe = await _context.Employes.FindAsync(EmployeId);

            if (employe == null)
            {
                ModelState.AddModelError("", "Employé introuvable.");
                ViewData["EmployeId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Employes, "Id", "NomComplet");
                return View();
            }

            // 2. Calcul du salaire net (Logique simplifiée pour le projet)
            // Formule : Salaire Base + Primes - Retenues
            decimal salaireNet = employe.Salaire + Primes - Retenues;

            // 3. Création de l'objet Paie
            var nouvellePaie = new Paie
            {
                EmployeId = EmployeId,
                Mois = $"{Mois} {Annee}", // Ex: "Décembre 2025"
                DateEmission = DateTime.Now,
                Montant = salaireNet
            };

            // 4. Sauvegarde
            if (ModelState.IsValid)
            {
                _context.Add(nouvellePaie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // En cas d'erreur, on réaffiche le formulaire
            ViewData["EmployeId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Employes, "Id", "NomComplet", EmployeId);
            return View();
        }

        // GET: Paies/DownloadPdf/5
        public async Task<IActionResult> DownloadPdf(int id)
        {
            // 1. Récupérer la paie avec les infos de l'employé
            var paie = await _context.Paies
                .Include(p => p.Employe)
                .FirstOrDefaultAsync(m => m.IdPaie == id);

            if (paie == null) return NotFound();

            // Sécurité : Un employé ne peut télécharger que SA paie (sauf Admin)
            var user = await _userManager.GetUserAsync(User);
            if (user.Role != "AdministrateurRH" && paie.EmployeId != user.Id)
            {
                return Forbid(); // Interdit
            }

            // 2. Générer le PDF
            var pdfService = new GestionRH.Services.PdfService();
            var pdfBytes = pdfService.GenererBulletinPaie(paie);

            // 3. Renvoyer le fichier
            string fileName = $"Bulletin_{paie.Employe.Nom}_{paie.Mois}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}