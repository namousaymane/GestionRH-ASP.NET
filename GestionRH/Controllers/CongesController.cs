using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionRH.Data;
using GestionRH.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using GestionRH.Services;

namespace GestionRH.Controllers
{
    [Authorize]
    public class CongesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly NotificationService _notificationService;

        public CongesController(ApplicationDbContext context, UserManager<Utilisateur> userManager, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // GET: Conges
        public async Task<IActionResult> Index(string searchString, string statutFilter, string typeFilter, string dateDebutFilter, string dateFinFilter)
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

            // FILTRES DE RECHERCHE
            if (!string.IsNullOrEmpty(searchString))
            {
                congesQuery = congesQuery.Where(c => 
                    c.Employe.Nom.Contains(searchString) || 
                    c.Employe.Prenom.Contains(searchString) ||
                    c.Type.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statutFilter))
            {
                congesQuery = congesQuery.Where(c => c.Statut == statutFilter);
            }

            if (!string.IsNullOrEmpty(typeFilter))
            {
                congesQuery = congesQuery.Where(c => c.Type == typeFilter);
            }

            if (!string.IsNullOrEmpty(dateDebutFilter) && DateTime.TryParse(dateDebutFilter, out DateTime dateDebut))
            {
                congesQuery = congesQuery.Where(c => c.DateDebut >= dateDebut);
            }

            if (!string.IsNullOrEmpty(dateFinFilter) && DateTime.TryParse(dateFinFilter, out DateTime dateFin))
            {
                congesQuery = congesQuery.Where(c => c.DateFin <= dateFin);
            }

            // Préparer les listes pour les dropdowns
            var statuts = await congesQuery.Select(c => c.Statut).Distinct().ToListAsync();
            var types = await _context.Conges.Select(c => c.Type).Distinct().ToListAsync();

            ViewBag.Statuts = statuts;
            ViewBag.Types = types;
            ViewBag.SearchString = searchString;
            ViewBag.StatutFilter = statutFilter;
            ViewBag.TypeFilter = typeFilter;
            ViewBag.DateDebutFilter = dateDebutFilter;
            ViewBag.DateFinFilter = dateFinFilter;

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
            if (user == null) return Challenge();

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

                // Notifier le manager si l'employé a un manager
                // Notifier le manager si l'employé a un manager
                if (user != null)
                {
                    var employe = await _context.Employes.FindAsync(user.Id);
                    if (employe != null && !string.IsNullOrEmpty(employe.ManagerId))
                    {
                        await _notificationService.CreerNotificationAsync(
                            employe.ManagerId,
                            "Nouvelle demande de congé",
                            $"{employe.NomComplet} a demandé un congé du {conge.DateDebut:dd/MM/yyyy} au {conge.DateFin:dd/MM/yyyy}",
                            "Conge",
                            Url.Action("Index", "Conges")
                        );
                    }

                    // Notifier les Administrateurs RH
                    var admins = await _userManager.GetUsersInRoleAsync("AdministrateurRH");
                    foreach (var admin in admins)
                    {
                        await _notificationService.CreerNotificationAsync(
                            admin.Id,
                            "Nouvelle demande de congé",
                            $"{employe?.NomComplet ?? user.UserName} a demandé un congé du {conge.DateDebut:dd/MM/yyyy} au {conge.DateFin:dd/MM/yyyy}",
                            "Conge",
                            Url.Action("Index", "Conges")
                        );
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(conge);
        }

        // GET: Conges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conge = await _context.Conges
                .Include(c => c.Employe)
                .FirstOrDefaultAsync(m => m.IdConge == id);

            if (conge == null)
            {
                return NotFound();
            }

            // Vérifier les autorisations
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.Role != "AdministrateurRH" && 
                user.Role != "Responsable" && 
                conge.EmployeId != user.Id)
            {
                return Forbid();
            }

            return View(conge);
        }

        // GET: Conges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conge = await _context.Conges.FindAsync(id);
            if (conge == null)
            {
                return NotFound();
            }

            // Vérifier les autorisations - seul l'employé peut modifier sa demande si elle est en attente
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (conge.EmployeId != user.Id || conge.Statut != "EnAttente")
            {
                if (user.Role != "AdministrateurRH")
                {
                    return Forbid();
                }
            }

            return View(conge);
        }

        // POST: Conges/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdConge,DateDebut,DateFin,Type,Statut,EmployeId")] Conge conge)
        {
            if (id != conge.IdConge)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Vérifier les autorisations
            var existingConge = await _context.Conges.FindAsync(id);
            if (existingConge == null) return NotFound();

            if (existingConge.EmployeId != user.Id || existingConge.Statut != "EnAttente")
            {
                if (user.Role != "AdministrateurRH")
                {
                    return Forbid();
                }
            }

            // Si c'est l'employé qui modifie, on garde le statut "EnAttente"
            if (existingConge.EmployeId == user.Id && user.Role != "AdministrateurRH")
            {
                conge.Statut = "EnAttente";
            }

            ModelState.Remove("Employe");
            if (conge.DateFin < conge.DateDebut)
            {
                ModelState.AddModelError("DateFin", "La date de fin doit être après la date de début.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingConge.DateDebut = conge.DateDebut;
                    existingConge.DateFin = conge.DateFin;
                    existingConge.Type = conge.Type;
                    if (user.Role == "AdministrateurRH")
                    {
                        existingConge.Statut = conge.Statut;
                    }

                    _context.Update(existingConge);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CongeExists(conge.IdConge))
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
            return View(conge);
        }

        // GET: Conges/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conge = await _context.Conges
                .Include(c => c.Employe)
                .FirstOrDefaultAsync(m => m.IdConge == id);

            if (conge == null)
            {
                return NotFound();
            }

            // Vérifier les autorisations - seul l'employé peut supprimer sa demande si elle est en attente
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (conge.EmployeId != user.Id || conge.Statut != "EnAttente")
            {
                if (user.Role != "AdministrateurRH")
                {
                    return Forbid();
                }
            }

            return View(conge);
        }

        // POST: Conges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conge = await _context.Conges.FindAsync(id);
            if (conge != null)
            {
                // Vérifier les autorisations
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

                if (conge.EmployeId != user.Id || conge.Statut != "EnAttente")
                {
                    if (user.Role != "AdministrateurRH")
                    {
                        return Forbid();
                    }
                }

                _context.Conges.Remove(conge);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // NOUVEAU : Action pour traiter une demande (Valider ou Refuser)
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
                if (user != null && user.Role == "Responsable")
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

                    // Notifier l'employé
                    await _notificationService.CreerNotificationAsync(
                        conge.EmployeId,
                        "Congé validé",
                        $"Votre demande de congé du {conge.DateDebut:dd/MM/yyyy} au {conge.DateFin:dd/MM/yyyy} a été validée.",
                        "Conge",
                        Url.Action("Details", "Conges", new { id = conge.IdConge })
                    );
                }
            }
            else if (decision == "Refuser")
            {
                conge.Statut = "Refuse";

                // Notifier l'employé
                await _notificationService.CreerNotificationAsync(
                    conge.EmployeId,
                    "Congé refusé",
                    $"Votre demande de congé du {conge.DateDebut:dd/MM/yyyy} au {conge.DateFin:dd/MM/yyyy} a été refusée.",
                    "Conge",
                    Url.Action("Details", "Conges", new { id = conge.IdConge })
                );
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // API pour le calendrier FullCalendar
        [HttpGet]
        public async Task<IActionResult> GetCalendarEvents()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            IQueryable<Conge> congesQuery = _context.Conges.Include(c => c.Employe);

            // Filtrer selon le rôle
            if (user.Role == "AdministrateurRH")
            {
                // L'admin voit tout
            }
            else if (user.Role == "Responsable")
            {
                congesQuery = congesQuery.Where(c => c.EmployeId == user.Id || c.Employe.ManagerId == user.Id);
            }
            else
            {
                congesQuery = congesQuery.Where(c => c.EmployeId == user.Id);
            }

            var conges = await congesQuery.ToListAsync();

            var events = conges.Select(c => new
            {
                id = c.IdConge,
                title = $"{c.Employe.NomComplet} - {c.Type}",
                start = c.DateDebut.ToString("yyyy-MM-dd"),
                end = c.DateFin.AddDays(1).ToString("yyyy-MM-dd"), // FullCalendar exclut le dernier jour
                backgroundColor = GetColorByStatut(c.Statut),
                borderColor = GetColorByStatut(c.Statut),
                textColor = "#ffffff",
                extendedProps = new
                {
                    employe = c.Employe.NomComplet,
                    type = c.Type,
                    statut = c.Statut,
                    duree = c.Duree
                }
            }).ToList();

            return Json(events);
        }

        private string GetColorByStatut(string statut)
        {
            return statut switch
            {
                "EnAttente" => "#ffc107", // Jaune
                "Valide" or "ValideManager" or "ApprouveRH" => "#28a745", // Vert
                "Refuse" or "RejeteManager" or "RejeteRH" => "#dc3545", // Rouge
                _ => "#6c757d" // Gris par défaut
            };
        }

        // GET: Conges/Calendrier
        public IActionResult Calendrier()
        {
            return View();
        }

        private bool CongeExists(int id)
        {
            return _context.Conges.Any(e => e.IdConge == id);
        }
    }
}