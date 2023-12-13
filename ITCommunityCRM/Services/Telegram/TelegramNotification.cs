using ITCommunityCRM.Data;
using ITCommunityCRM.Data.Models;
using ITCommunityCRM.Data.Models.Consts;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace ITCommunityCRM.Services.Telegram
{
    public class TelegramNotification
    {
        private ITelegramBotClient _telegramBotClient { get; set; }
        private readonly ITCommunityCRMDbContext _context;
        private readonly TemplateServise _templateServise;

        public TelegramNotification(ITelegramBotClient telegramBotClient, ITCommunityCRMDbContext context, 
            TemplateServise templateServise)
        {
            _telegramBotClient = telegramBotClient;
            _context = context;
            _templateServise = templateServise;
        }

        public async Task NotificateAsync(Event e)
        {
            var tgUsers = _context.UserLogins.Include(x => x.UserId)
                .Where(x => x.LoginProvider == UserLoginInfoConst.TelegramLoginProvider).ToList();
            var template = e.NotificationTemplate.MessageTemplate;

            foreach (var tgUser in tgUsers) {
                //_templateServise.GetMessage(); TODO
                await _telegramBotClient.SendTextMessageAsync(tgUser.ProviderKey, template);
            }
        }
    }
}
