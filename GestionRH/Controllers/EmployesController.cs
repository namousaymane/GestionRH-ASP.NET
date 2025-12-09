using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionRH.Data;
using GestionRH.Models;
using System.Linq;
using System.Threading.Tasks;

namespace GestionRH.Controllers
{
    public class EmployesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employes
        public async Task<IActionResult> Index()
        {
            // On récupère uniquement les utilisateurs qui sont des "Employés"
            // Grâce à l'héritage, EF Core le fait automatiquement via le DbSet Employes
            var employes = await _context.Employes.ToListAsync();
            return View(employes);
        }

        // GET: Employes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Prenom,Email,Poste,Salaire,DateEmbauche")] Employe employe)
        {
            // On force le rôle et le type par défaut
            employe.UserName = employe.Email; // Le username est l'email
            employe.Role = "Employe";
            employe.SecurityStamp = Guid.NewGuid().ToString(); // Nécessaire pour Identity

            // Note: Ici on ne gère pas le mot de passe pour l'instant, 
            // l'employé devra faire "mot de passe oublié" ou on le générera plus tard.

            if (ModelState.IsValid)
            {
                _context.Add(employe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employe);
        }
    }
}