using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionRH.Data;
using GestionRH.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace GestionRH.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        // On injecte le DbContext pour accéder aux données
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Si l'utilisateur n'est pas connecté, on affiche l'accueil public simple
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }

            // --- CALCUL DES INDICATEURS (KPI) ---

            // 1. Effectif total (Nombre d'employés)
            ViewData["TotalEmployes"] = await _context.Employes.CountAsync();

            // 2. Demandes de congés en attente (Global)
            ViewData["CongesEnAttente"] = await _context.Conges.CountAsync(c => c.Statut == "EnAttente");

            // 3. Masse salariale (Somme des salaires de base)
            ViewData["MasseSalariale"] = await _context.Employes.SumAsync(e => e.Salaire);

            // 4. Derniers congés validés (pour l'affichage rapide)
            ViewData["DerniersConges"] = await _context.Conges
                                               .Include(c => c.Employe)
                                               .Where(c => c.Statut == "Valide")
                                               .OrderByDescending(c => c.DateDebut)
                                               .Take(5)
                                               .ToListAsync();

            return View();
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
}