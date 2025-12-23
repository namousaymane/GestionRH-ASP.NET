using GestionRH.Data;
using GestionRH.Hubs;
using GestionRH.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GestionRH.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreerNotificationAsync(string userId, string titre, string message, string type, string? lienAction = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Titre = titre,
                Message = message,
                Type = type,
                LienAction = lienAction,
                EstLue = false,
                DateCreation = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Envoyer la notification en temps réel via SignalR
            await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new
            {
                id = notification.Id,
                titre = notification.Titre,
                message = notification.Message,
                type = notification.Type,
                dateCreation = notification.DateCreation,
                lienAction = notification.LienAction
            });
        }

        public async Task<List<Notification>> GetNotificationsAsync(string userId, bool nonLuesSeulement = false)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (nonLuesSeulement)
            {
                query = query.Where(n => !n.EstLue);
            }

            return await query
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();
        }

        public async Task MarquerCommeLueAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.EstLue = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetNombreNotificationsNonLuesAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.EstLue);
        }
    }
}

