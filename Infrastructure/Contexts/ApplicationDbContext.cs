using Infrastructure.Constants;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,
            ApplicationRole,
            string,
            IdentityUserClaim<string>,
            ApplicationUserRole<string>,
            IdentityUserLogin<string>,
            ApplicationRoleClaim,
            IdentityUserToken<string>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);

        //    //builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        //    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("Users", SchemaNames.Identity);
            });

            modelBuilder.Entity<ApplicationRole>(b =>
            {
                b.ToTable("Roles", SchemaNames.Identity);
            });

            modelBuilder.Entity<ApplicationRoleClaim>(b =>
            {
                b.ToTable("RoleClaims", SchemaNames.Identity);
            });

            modelBuilder.Entity<IdentityUserRole<string>>(b =>
            {
                b.ToTable("UserRoles", SchemaNames.Identity);
                b.HasKey(r => new { r.UserId, r.RoleId });
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("UserClaims", SchemaNames.Identity);
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable("UserLogins", SchemaNames.Identity);
            });

            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable("UserTokens", SchemaNames.Identity);
            });
        }
        //DbSets
    }
}
