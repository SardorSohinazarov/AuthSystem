using Auth.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Auth.API.Domain
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            Database.Migrate();
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>()
                .HasMany(x => x.Permissions)
                .WithMany(x => x.Roles)
                .UsingEntity<RolePermission>();

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
