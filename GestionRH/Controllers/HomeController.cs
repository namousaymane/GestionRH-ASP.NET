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
                    .CountAsync(c => c.Employe.ManagerId == user.Id || c.EmployeId == user.Id);
                stats.CongesEnAttente = await _context.Conges
                    .CountAsync(c => (c.Employe.ManagerId == user.Id || c.EmployeId == user.Id) && c.Statut == "EnAttente");
                stats.CongesValides = await _context.Conges
                    .CountAsync(c => (c.Employe.ManagerId == user.Id || c.EmployeId == user.Id) && c.Statut == "Valide");
                stats.TotalPaies = await _context.Paies
                    .CountAsync(p => p.Employe.ManagerId == user.Id);
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
