using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionRH.Data;
using GestionRH.Models;
using Microsoft.AspNetCore.Identity;

namespace GestionRH.Controllers
{
    public class CongesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;

        public CongesController(ApplicationDbContext context, UserManager<Utilisateur> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Conges
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            IQueryable<Conge> congesQuery = _context.Conges.Include(c => c.Employe);

            // LOGIQUE DE FILTRAGE SELON LE RÔLE
            if (user.Role == "AdministrateurRH")
            {
                // L'admin voit TOUT (pour la validation finale et l'historique)
                // Pas de filtre "Where"
            }
            else if (user.Role == "Responsable") // Manager
            {
                // Le manager voit ses congés + ceux de son équipe (ceux dont il est le manager)
                // Note: On assume que user.Id est utilisé comme ManagerId dans la table Employés
                congesQuery = congesQuery.Where(c => c.EmployeId == user.Id || c.Employe.ManagerId == user.Id);
            }
            else // Employé simple
            {
                // Ne voit que SES demandes
                congesQuery = congesQuery.Where(c => c.EmployeId == user.Id);
            }

            var listeConges = await congesQuery.OrderByDescending(c => c.DateDebut).ToListAsync();
            return View(listeConges);
        }

        // GET: Conges/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Conges/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DateDebut,DateFin,Type")] Conge conge)
        {
            var user = await _userManager.GetUserAsync(User);

            conge.EmployeId = user.Id;
            conge.Statut = "EnAttente"; // Statut initial

            // On ignore les erreurs de validation pour les champs qu'on remplit nous-mêmes
            ModelState.Remove("Employe");
            ModelState.Remove("EmployeId");
            ModelState.Remove("Statut");

            if (conge.DateFin < conge.DateDebut)
            {
                ModelState.AddModelError("DateFin", "La date de fin doit être après la date de début.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(conge);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(conge);
        }

        // NOUVEAU : Action pour traiter une demande (Valider ou Refuser)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Traiter(int id, string decision)
        {
            var conge = await _context.Conges.FindAsync(id);
            if (conge == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            // Logique de validation à 2 niveaux (Manager -> RH)
            if (decision == "Valider")
            {
                if (user.Role == "Responsable")
                {
                    conge.Statut = "ValideManager"; // 1er niveau
                }
                else if (user.Role == "AdministrateurRH")
                {
                    conge.Statut = "Valide"; // Validation finale
                    // ICI : On pourrait déduire du solde de l'employé
                }
            }
            else if (decision == "Refuser")
            {
                conge.Statut = "Refuse";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}