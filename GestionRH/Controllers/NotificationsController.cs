using GestionRH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GestionRH.Models;

namespace GestionRH.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly UserManager<Utilisateur> _userManager;

        public NotificationsController(NotificationService notificationService, UserManager<Utilisateur> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var notifications = await _notificationService.GetNotificationsAsync(user.Id);
            return View(notifications);
        }

        // GET: Notifications/GetUnreadCount
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { count = 0 });

            var count = await _notificationService.GetNombreNotificationsNonLuesAsync(user.Id);
            return Json(new { count });
        }

        // POST: Notifications/MarkAsRead/5
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _notificationService.MarquerCommeLueAsync(id, user.Id);
            return Ok();
        }

        // POST: Notifications/MarkAllAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notifications = await _notificationService.GetNotificationsAsync(user.Id, nonLuesSeulement: true);
            foreach (var notification in notifications)
            {
                await _notificationService.MarquerCommeLueAsync(notification.Id, user.Id);
            }

            return Ok();
        }
    }
}

