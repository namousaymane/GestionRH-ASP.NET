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

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Conges/Traiter
        public async Task<IActionResult> Traiter(int id, string decision)
        {
            // On inclut l'employé pour pouvoir modifier son solde
            var conge = await _context.Conges.Include(c => c.Employe).FirstOrDefaultAsync(c => c.IdConge == id);

            if (conge == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (decision == "Valider")
            {
                if (user.Role == "Responsable")
                {
                    conge.Statut = "ValideManager";
                }
                else if (user.Role == "AdministrateurRH")
                {
                    // VÉRIFICATION DU SOLDE AVANT VALIDATION
                    int duree = (conge.DateFin - conge.DateDebut).Days + 1;

                    if (conge.Employe.SoldeConges >= duree)
                    {
                        conge.Statut = "Valide";
                        // DÉCRÉMENTATION
                        conge.Employe.SoldeConges -= duree;
                    }
                    else
                    {
                        // Optionnel : Gérer l'erreur si solde insuffisant (pour l'instant on refuse ou on laisse passer en négatif ?)
                        // Pour faire simple : on laisse passer ou on met un message, 
                        // mais techniquement on retire les jours :
                        conge.Statut = "Valide";
                        conge.Employe.SoldeConges -= duree;
                    }
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