using ITCommunityCRM.Data;
using ITCommunityCRM.Data.Models;
using ITCommunityCRM.Data.Models.Consts;
using ITCommunityCRM.Services.Telegram;
using Microsoft.EntityFrameworkCore;

namespace ITCommunityCRM.Services
{
    public class NotificationService
    {
        private readonly ITCommunityCRMDbContext _context;
        private readonly TelegramNotification _telegramNotification;
        private readonly EmailNotification _emailNotification;

        public NotificationService(TelegramNotification telegramNotification, ITCommunityCRMDbContext context,
            EmailNotification emailNotification)
        {
            _telegramNotification = telegramNotification;
            _context = context;
            _emailNotification = emailNotification;
        }

        public async Task NotificateAsync(int eventId)
        {
            var e = await _context.Events
                .Include(x => x.NotificationTemplate)
                .FirstOrDefaultAsync(x => x.Id == eventId);

            switch (e.NotificationTemplate.NotificationType.Type)
            {
                case NotificationTypeConst.All:
                {
                    await _emailNotification.NotificateAsync(e);
                    await _telegramNotification.NotificateAsync(e);
                    break;
                }
                case NotificationTypeConst.Email:
                {
                    await _emailNotification.NotificateAsync(e);
                    break;
                }
                case NotificationTypeConst.Telegram: 
                {
                    await _telegramNotification.NotificateAsync(e);
                    break;
                };
                default: break;  
            }
        }
    }
}
