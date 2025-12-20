using GestionRH.Data;
using GestionRH.Models;
using GestionRH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestionRH.Controllers
{
    [Authorize]
    public class PointagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly NotificationService _notificationService;

        public PointagesController(ApplicationDbContext context, UserManager<Utilisateur> userManager, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // GET: Pointages
        public async Task<IActionResult> Index(string searchString, DateTime? dateDebut, DateTime? dateFin)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            IQueryable<Pointage> pointagesQuery = _context.Pointages.Include(p => p.Employe);

            // Filtrer selon le rôle
            if (user.Role == "AdministrateurRH")
            {
                // L'admin voit tout
            }
            else if (user.Role == "Responsable")
            {
                pointagesQuery = pointagesQuery.Where(p => p.Employe.ManagerId == user.Id);
            }
            else
            {
                pointagesQuery = pointagesQuery.Where(p => p.EmployeId == user.Id);
            }

            // Filtres
            if (!string.IsNullOrEmpty(searchString))
            {
                pointagesQuery = pointagesQuery.Where(p => 
                    p.Employe.Nom.Contains(searchString) || 
                    p.Employe.Prenom.Contains(searchString));
            }

            if (dateDebut.HasValue)
            {
                pointagesQuery = pointagesQuery.Where(p => p.DatePointage >= dateDebut.Value);
            }

            if (dateFin.HasValue)
            {
                pointagesQuery = pointagesQuery.Where(p => p.DatePointage <= dateFin.Value);
            }

            ViewBag.SearchString = searchString;
            ViewBag.DateDebut = dateDebut?.ToString("yyyy-MM-dd");
            ViewBag.DateFin = dateFin?.ToString("yyyy-MM-dd");

            var pointages = await pointagesQuery.OrderByDescending(p => p.DatePointage).ToListAsync();
            return View(pointages);
        }

        // GET: Pointages/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Si c'est un employé, pointer pour lui-même
            if (user.Role == "Employe")
            {
                // Vérifier s'il y a déjà un pointage aujourd'hui
                var pointageExistant = await _context.Pointages
                    .FirstOrDefaultAsync(p => p.EmployeId == user.Id && 
                                             p.DatePointage.Date == DateTime.Today);
                
                if (pointageExistant != null)
                {
                    TempData["Message"] = "Vous avez déjà pointé aujourd'hui.";
                    return RedirectToAction(nameof(Index));
                }

                var pointage = new Pointage
                {
                    EmployeId = user.Id,
                    DatePointage = DateTime.Today,
                    HeureArrivee = DateTime.Now
                };
                _context.Add(pointage);
                await _context.SaveChangesAsync();

                // Notifier le manager
                var employe = await _context.Employes.FindAsync(user.Id);
                if (employe != null && !string.IsNullOrEmpty(employe.ManagerId))
                {
                    await _notificationService.CreerNotificationAsync(
                        employe.ManagerId,
                        "Pointage d'arrivée",
                        $"{employe.NomComplet} a pointé son arrivée aujourd'hui à {pointage.HeureArrivee.Value:HH:mm}.",
                        "Pointage",
                        $"/Pointages/Index"
                    );
                }

                // Notifier les Administrateurs RH
                var admins = await _userManager.GetUsersInRoleAsync("AdministrateurRH");
                foreach (var admin in admins)
                {
                    await _notificationService.CreerNotificationAsync(
                        admin.Id,
                        "Pointage d'arrivée",
                        $"{employe?.NomComplet ?? user.UserName} a pointé son arrivée aujourd'hui à {pointage.HeureArrivee.Value:HH:mm}.",
                        "Pointage",
                        $"/Pointages/Index"
                    );
                }

                TempData["SuccessMessage"] = "Pointage d'arrivée enregistré avec succès.";
                return RedirectToAction(nameof(Index));
            }

            // Pour Admin/RH, permettre de sélectionner l'employé
            var employes = await _context.Employes.ToListAsync();
            ViewBag.Employes = employes.Select(e => new SelectListItem
            {
                Value = e.Id,
                Text = e.NomComplet
            }).ToList();
            return View();
        }

        // POST: Pointages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeId,DatePointage,HeureArrivee,HeureDepart,EstAbsent,Remarques")] Pointage pointage)
        {
            if (ModelState.IsValid)
            {
                // Calculer la durée si les heures sont fournies
                if (pointage.HeureArrivee.HasValue && pointage.HeureDepart.HasValue)
                {
                    pointage.DureeTravail = pointage.HeureDepart.Value - pointage.HeureArrivee.Value;
                    
                    // Calculer les heures supplémentaires (plus de 8 heures)
                    var heuresNormales = TimeSpan.FromHours(8);
                    if (pointage.DureeTravail.Value > heuresNormales)
                    {
                        pointage.HeuresSupplementaires = (decimal)(pointage.DureeTravail.Value - heuresNormales).TotalHours;
                    }
                }

                _context.Add(pointage);
                await _context.SaveChangesAsync();

                // Notifier l'employé
                await _notificationService.CreerNotificationAsync(
                    pointage.EmployeId,
                    "Pointage enregistré",
                    $"Un pointage a été enregistré pour vous le {pointage.DatePointage:dd/MM/yyyy}.",
                    "Pointage",
                    $"/Pointages/Index"
                );

                // Notifier le manager si assigné
                // Notifier le manager si assigné
                var employe = await _context.Employes.FindAsync(pointage.EmployeId);
                if (employe != null && !string.IsNullOrEmpty(employe.ManagerId))
                {
                    await _notificationService.CreerNotificationAsync(
                        employe.ManagerId,
                        "Pointage enregistré",
                        $"Un pointage a été enregistré pour {employe.NomComplet} le {pointage.DatePointage:dd/MM/yyyy}.",
                        "Pointage",
                        $"/Pointages/Index"
                    );
                }

                // Notifier les Administrateurs RH
                var admins = await _userManager.GetUsersInRoleAsync("AdministrateurRH");
                foreach (var admin in admins)
                {
                    await _notificationService.CreerNotificationAsync(
                        admin.Id,
                        "Pointage enregistré",
                        $"Un pointage a été enregistré pour {employe?.NomComplet ?? "un employé"} le {pointage.DatePointage:dd/MM/yyyy}.",
                        "Pointage",
                        $"/Pointages/Index"
                    );
                }

                TempData["SuccessMessage"] = "Pointage créé avec succès.";
                return RedirectToAction(nameof(Index));
            }

            var employes = await _context.Employes.ToListAsync();
            ViewBag.Employes = employes.Select(e => new SelectListItem
            {
                Value = e.Id,
                Text = e.NomComplet
            }).ToList();
            return View(pointage);
        }

        // POST: Pointages/PointageDepart
        [HttpPost]
        public async Task<IActionResult> PointageDepart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var pointageAujourdhui = await _context.Pointages
                .FirstOrDefaultAsync(p => p.EmployeId == user.Id && 
                                         p.DatePointage.Date == DateTime.Today &&
                                         !p.HeureDepart.HasValue);

            if (pointageAujourdhui != null)
            {
                pointageAujourdhui.HeureDepart = DateTime.Now;
                
                if (pointageAujourdhui.HeureArrivee.HasValue)
                {
                    pointageAujourdhui.DureeTravail = pointageAujourdhui.HeureDepart.Value - pointageAujourdhui.HeureArrivee.Value;
                    
                    var heuresNormales = TimeSpan.FromHours(8);
                    if (pointageAujourdhui.DureeTravail.Value > heuresNormales)
                    {
                        pointageAujourdhui.HeuresSupplementaires = (decimal)(pointageAujourdhui.DureeTravail.Value - heuresNormales).TotalHours;
                    }
                }

                await _context.SaveChangesAsync();

                // Notifier le manager
                var employe = await _context.Employes.FindAsync(user.Id);
                if (employe != null && !string.IsNullOrEmpty(employe.ManagerId))
                {
                    await _notificationService.CreerNotificationAsync(
                        employe.ManagerId,
                        "Pointage de départ",
                        $"{employe.NomComplet} a pointé son départ aujourd'hui à {pointageAujourdhui.HeureDepart.Value:HH:mm}.",
                        "Pointage",
                        $"/Pointages/Index"
                    );
                }

                // Notifier les Administrateurs RH
                var admins = await _userManager.GetUsersInRoleAsync("AdministrateurRH");
                foreach (var admin in admins)
                {
                    await _notificationService.CreerNotificationAsync(
                        admin.Id,
                        "Pointage de départ",
                        $"{employe?.NomComplet ?? user.UserName} a pointé son départ aujourd'hui à {pointageAujourdhui.HeureDepart.Value:HH:mm}.",
                        "Pointage",
                        $"/Pointages/Index"
                    );
                }

                return Json(new { success = true, message = "Pointage de départ enregistré" });
            }

            return Json(new { success = false, message = "Aucun pointage d'arrivée trouvé pour aujourd'hui" });
        }

        // GET: Pointages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (id == null)
            {
                return NotFound();
            }

            var pointage = await _context.Pointages
                .Include(p => p.Employe)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pointage == null)
            {
                return NotFound();
            }

            // Vérifier les permissions
            if (user.Role == "Employe" && pointage.EmployeId != user.Id)
            {
                return Forbid();
            }
            else if (user.Role == "Responsable" && pointage.Employe.ManagerId != user.Id)
            {
                return Forbid();
            }

            return View(pointage);
        }

        // GET: Pointages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Seuls les AdministrateursRH peuvent modifier les pointages
            if (user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var pointage = await _context.Pointages
                .Include(p => p.Employe)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pointage == null)
            {
                return NotFound();
            }

            var employes = await _context.Employes.ToListAsync();
            ViewBag.Employes = employes.Select(e => new SelectListItem
            {
                Value = e.Id,
                Text = e.NomComplet
            }).ToList();
            return View(pointage);
        }

        // POST: Pointages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeId,DatePointage,HeureArrivee,HeureDepart,EstAbsent,Remarques")] Pointage pointage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Seuls les AdministrateursRH peuvent modifier les pointages
            if (user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            if (id != pointage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Recalculer la durée si les heures sont fournies
                    if (pointage.HeureArrivee.HasValue && pointage.HeureDepart.HasValue)
                    {
                        pointage.DureeTravail = pointage.HeureDepart.Value - pointage.HeureArrivee.Value;
                        
                        var heuresNormales = TimeSpan.FromHours(8);
                        if (pointage.DureeTravail.Value > heuresNormales)
                        {
                            pointage.HeuresSupplementaires = (decimal)(pointage.DureeTravail.Value - heuresNormales).TotalHours;
                        }
                        else
                        {
                            pointage.HeuresSupplementaires = 0;
                        }
                    }

                    _context.Update(pointage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PointageExists(pointage.Id))
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

            var employes = await _context.Employes.ToListAsync();
            ViewBag.Employes = employes.Select(e => new SelectListItem
            {
                Value = e.Id,
                Text = e.NomComplet
            }).ToList();
            return View(pointage);
        }

        // GET: Pointages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Seuls les AdministrateursRH peuvent supprimer les pointages
            if (user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var pointage = await _context.Pointages
                .Include(p => p.Employe)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pointage == null)
            {
                return NotFound();
            }

            return View(pointage);
        }

        // POST: Pointages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Seuls les AdministrateursRH peuvent supprimer les pointages
            if (user.Role != "AdministrateurRH")
            {
                return Forbid();
            }

            var pointage = await _context.Pointages.FindAsync(id);
            if (pointage != null)
            {
                _context.Pointages.Remove(pointage);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PointageExists(int id)
        {
            return _context.Pointages.Any(e => e.Id == id);
        }
    }
}

