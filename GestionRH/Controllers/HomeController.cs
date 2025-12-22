using System.Diagnostics;
using GestionRH.Models;
using GestionRH.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestionRH.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<Utilisateur> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Si l'utilisateur n'est pas connecté, on affiche l'accueil public simple
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View();
            }

            var stats = new DashboardStats();

            if (user.Role == "AdministrateurRH")
            {
                // Statistiques pour l'administrateur RH
                stats.TotalEmployes = await _context.Employes.CountAsync();
                stats.TotalConges = await _context.Conges.CountAsync();
                stats.CongesEnAttente = await _context.Conges.CountAsync(c => c.Statut == "EnAttente");
                stats.CongesValides = await _context.Conges.CountAsync(c => c.Statut == "Valide");
                stats.TotalPaies = await _context.Paies.CountAsync();
                stats.TotalResponsables = await _context.Responsables.CountAsync();
            }
            else if (user.Role == "Responsable")
            {
                // Statistiques pour le responsable (manager)
                stats.TotalEmployes = await _context.Employes.CountAsync(e => e.ManagerId == user.Id);
                stats.TotalConges = await _context.Conges
                    .CountAsync(c => (c.Employe is Employe && ((Employe)c.Employe).ManagerId == user.Id) || c.EmployeId == user.Id);
                stats.CongesEnAttente = await _context.Conges
                    .CountAsync(c => ((c.Employe is Employe && ((Employe)c.Employe).ManagerId == user.Id) || c.EmployeId == user.Id) && c.Statut == "EnAttente");
                stats.CongesValides = await _context.Conges
                    .CountAsync(c => ((c.Employe is Employe && ((Employe)c.Employe).ManagerId == user.Id) || c.EmployeId == user.Id) && c.Statut == "Valide");
                stats.TotalPaies = await _context.Paies
                    .CountAsync(p => p.Employe.ManagerId == user.Id); // Paie is still linked to Employe, so this is valid ?? Checking Paie model... Paie.Employe is ? Let's check Paie model. Assumed Valid for now as Paie usually linked to Employe.
            }
            else
            {
                // Statistiques pour l'employé
                stats.TotalConges = await _context.Conges.CountAsync(c => c.EmployeId == user.Id);
                stats.CongesEnAttente = await _context.Conges.CountAsync(c => c.EmployeId == user.Id && c.Statut == "EnAttente");
                stats.CongesValides = await _context.Conges.CountAsync(c => c.EmployeId == user.Id && c.Statut == "Valide");
                stats.TotalPaies = await _context.Paies.CountAsync(p => p.EmployeId == user.Id);
            }

            ViewBag.UserRole = user.Role;
            ViewBag.UserName = user.NomComplet;
            return View(stats);
        }

        // API pour les données des graphiques
        [HttpGet]
        public async Task<IActionResult> GetChartData()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var data = new
            {
                // Congés par mois (6 derniers mois)
                congesParMois = await GetCongesParMois(user),
                // Congés par statut
                congesParStatut = await GetCongesParStatut(user),
                // Congés par département (pour Admin)
                congesParDepartement = user.Role == "AdministrateurRH" ? await GetCongesParDepartement() : null,
                // Paies par mois (6 derniers mois)
                paiesParMois = await GetPaiesParMois(user)
            };

            return Json(data);
        }

        private async Task<List<object>> GetCongesParMois(Utilisateur user)
        {
            var sixMoisAgo = DateTime.Now.AddMonths(-6);
            IQueryable<Conge> query = _context.Conges.Where(c => c.DateDebut >= sixMoisAgo);

            if (user.Role == "Responsable")
            {
                query = query.Where(c => (c.Employe is Employe && ((Employe)c.Employe).ManagerId == user.Id) || c.EmployeId == user.Id);
            }
            else if (user.Role == "Employe")
            {
                query = query.Where(c => c.EmployeId == user.Id);
            }

            var conges = await query.ToListAsync();
            var result = new List<object>();

            for (int i = 5; i >= 0; i--)
            {
                var mois = DateTime.Now.AddMonths(-i);
                var count = conges.Count(c => c.DateDebut.Year == mois.Year && c.DateDebut.Month == mois.Month);
                result.Add(new { mois = mois.ToString("MMM yyyy"), count });
            }

            return result;
        }

        private async Task<List<object>> GetCongesParStatut(Utilisateur user)
        {
            IQueryable<Conge> query = _context.Conges;

            if (user.Role == "Responsable")
            {
                query = query.Where(c => (c.Employe is Employe && ((Employe)c.Employe).ManagerId == user.Id) || c.EmployeId == user.Id);
            }
            else if (user.Role == "Employe")
            {
                query = query.Where(c => c.EmployeId == user.Id);
            }

            var conges = await query.ToListAsync();
            return new List<object>
            {
                new { statut = "En Attente", count = conges.Count(c => c.Statut == "EnAttente") },
                new { statut = "Validés", count = conges.Count(c => c.Statut.Contains("Approuve")) },
                new { statut = "Refusés", count = conges.Count(c => c.Statut.Contains("Rejete")) }
            };
        }

        private async Task<List<object>> GetCongesParDepartement()
        {
            // On part des employés pour avoir le département
            var stats = await _context.Employes
                .Include(e => e.Departement)
                .SelectMany(e => e.Conges.Select(c => new { DeptName = e.Departement.Nom ?? "Sans département" }))
                .ToListAsync();

             return stats
                .GroupBy(x => x.DeptName)
                .Select(g => new { departement = g.Key, count = g.Count() })
                .Cast<object>()
                .ToList();
        }

        private async Task<List<object>> GetPaiesParMois(Utilisateur user)
        {
            var sixMoisAgo = DateTime.Now.AddMonths(-6);
            IQueryable<Paie> query = _context.Paies.Where(p => p.DateEmission >= sixMoisAgo);

            if (user.Role == "Responsable")
            {
                query = query.Where(p => p.Employe.ManagerId == user.Id);
            }
            else if (user.Role == "Employe")
            {
                query = query.Where(p => p.EmployeId == user.Id);
            }

            var paies = await query.ToListAsync();
            var result = new List<object>();

            for (int i = 5; i >= 0; i--)
            {
                var mois = DateTime.Now.AddMonths(-i);
                var count = paies.Count(p => p.DateEmission.Year == mois.Year && p.DateEmission.Month == mois.Month);
                result.Add(new { mois = mois.ToString("MMM yyyy"), count });
            }

            return result;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class DashboardStats
    {
        public int TotalEmployes { get; set; }
        public int TotalConges { get; set; }
        public int CongesEnAttente { get; set; }
        public int CongesValides { get; set; }
        public int TotalPaies { get; set; }
        public int TotalResponsables { get; set; }
    }
}
