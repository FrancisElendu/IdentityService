using Infrastructure.Constants;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Contexts
{
    //internal class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
    //{
    //    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    //    {
    //        builder.ToTable("Users", SchemaNames.Identity);
    //    }

    //    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    //    {
    //        builder.ToTable("Roles", SchemaNames.Identity);
    //    }

    //    public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
    //    {
    //        builder.ToTable("RoleClaims", SchemaNames.Identity);
    //    }

    //    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    //    {
    //        builder.ToTable("UserRoles", SchemaNames.Identity);
    //    }

    //    public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
    //    {
    //        builder.ToTable("UserClaims", SchemaNames.Identity);
    //    }

    //    public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
    //    {
    //        builder.ToTable("UserLogins", SchemaNames.Identity);
    //    }

    //    public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
    //    {
    //        builder.ToTable("UserTokens", SchemaNames.Identity);
    //    }
    //}
}
