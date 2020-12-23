using KAI_Schedule.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KAI_Schedule.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public ApplicationDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConfigManager.Config.ConnectionString);
        }

        public virtual DbSet<ScheduleDbEntry> Schedules { get; set; }
        public virtual DbSet<ChatContext> ChatContexts { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            ConfigManager.Initialize();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(ConfigManager.Config.ConnectionString);

            return new ApplicationDbContext(builder.Options);
        }
    }
}
