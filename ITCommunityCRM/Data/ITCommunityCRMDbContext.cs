using ITCommunityCRM.Data.Models;
using ITCommunityCRM.Data.Models.Consts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITCommunityCRM.Data
{
    public class ITCommunityCRMDbContext : IdentityDbContext<User>
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<NotificationTemplate> NotificationMessageTemplates { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<TelegramChat> TelegramChats { get; set; }

        public ITCommunityCRMDbContext(DbContextOptions<ITCommunityCRMDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NotificationType>()
            .HasData(
            new NotificationType[]
            {
                new NotificationType { Id = 1, Type = NotificationTypeConst.All},
                new NotificationType { Id = 2, Type = NotificationTypeConst.Email},
                new NotificationType { Id = 3, Type = NotificationTypeConst.Telegram},
            });

            modelBuilder.Entity<NotificationTemplate>()
            .HasData(
            new NotificationTemplate[]
            {
                new NotificationTemplate { Id = 1, MessageTemplate = "Hi", Title = "Title", NotificationTypeId = 3},
            });


        }
    }
}
