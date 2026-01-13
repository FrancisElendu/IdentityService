using AuthLibrary.Constants.Authentication;
using Infrastructure.Constants;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts
{
    public class ApplicationDbSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public ApplicationDbSeeder(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task SeedIdentityDatabaseAsync()
        {
            await CheckAndApplyPendingMigrationAsync();
            await SeedRolesAsync();
            await SeedAdminUserAsync();
            await SeedBasicUserAsync();
        }

        private async Task CheckAndApplyPendingMigrationAsync()
        {
            if (_context.Database.GetPendingMigrations().Any())
            {
                await _context.Database.MigrateAsync();
            }
        }

        private async Task SeedRolesAsync()
        {
            // Implement role seeding logic here
            foreach (var roleName in AppRoles.DefaultRoles)
            {
                //if (!await _roleManager.RoleExistsAsync(roleName))
                //{
                //    var role = new ApplicationRole { Name = roleName };
                //    await _roleManager.CreateAsync(role);
                //}

                if (await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName) is not ApplicationRole role)
                {
                    role = new ApplicationRole
                    {
                        Name = roleName,
                        Description = $"{roleName} Role.",
                        NormalizedName = roleName.ToUpper()
                    };
                    await _roleManager.CreateAsync(role);
                }

                //assign permissions to roles here
                if (roleName == AppRoles.Basic)
                {
                    await AssignPermissionsToRoleAsync(role, AppPermissions.BasicPermissions);
                }
                else if(roleName == AppRoles.Admin)
                {
                    await AssignPermissionsToRoleAsync(role, AppPermissions.AdminPermissions);
                }

            }
        }

        private async Task AssignPermissionsToRoleAsync(ApplicationRole role, 
            IReadOnlyList<AppPermission> permissions)
        {
            var currentlyAssignedClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var permission in permissions)
            {
                if (!currentlyAssignedClaims.Any(cl => cl.Type == AppClaim.Permission && cl.Value == permission.Name))
                {
                    await _context.RoleClaims.AddAsync(new ApplicationRoleClaim
                    {
                        RoleId = role.Id,
                        ClaimType = AppClaim.Permission,
                        ClaimValue = permission.Name,
                        Description = permission.Description,
                        Group = permission.Group
                    });
                    await _context.SaveChangesAsync();
                }
            } 
        }


        //seed admin user
        private async Task SeedAdminUserAsync()
        {
            //admin user
            var user = new ApplicationUser
            {
                FirstName = "System",
                LastName = "Admin",
                Email = AppCredentials.Email,
                UserName = AppCredentials.Email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                PhoneNumber = "111-111-1111",
                NormalizedEmail = AppCredentials.Email.ToUpper(),
                NormalizedUserName = AppCredentials.Email.ToUpper(),
                IsActive = true,
                RefreshToken = string.Empty
            };
            if(await _userManager.FindByEmailAsync(AppCredentials.Email) is null)
            {
                var password = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = password.HashPassword(user, AppCredentials.Password);

                await _userManager.CreateAsync(user);

                user = await _userManager.FindByEmailAsync(AppCredentials.Email);

                //assign role(s) to user
                if (!await _userManager.IsInRoleAsync(user, AppRoles.Admin)
                    && !await _userManager.IsInRoleAsync(user, AppRoles.Basic))
                {
                    await _userManager.AddToRolesAsync(user, AppRoles.DefaultRoles);
                }
            }
        }


        //seed basic user
        private async Task SeedBasicUserAsync()
        {
            //basic user
            var email = "john.doe@email.com";
            var user = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe",
                Email = email,
                UserName = email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                PhoneNumber = "222-222-2222",
                NormalizedEmail = email.ToUpper(),
                NormalizedUserName = email.ToUpper(),
                IsActive = true,
                RefreshToken = string.Empty
            };
            if (await _userManager.FindByEmailAsync(email) is null)
            {
                var password = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = password.HashPassword(user, AppCredentials.Password);

                await _userManager.CreateAsync(user);

                user = await _userManager.FindByEmailAsync(email);

                //assign role(s) to user
                if (!await _userManager.IsInRoleAsync(user, AppRoles.Basic))
                {
                    await _userManager.AddToRoleAsync(user, AppRoles.Basic);
                }

                //var result = await _userManager.CreateAsync(user);
                //if (result.Succeeded)
                //{
                //    //assign role(s) to user
                //    if (!await _userManager.IsInRoleAsync(user, AppRoles.Basic))
                //    {
                //        await _userManager.AddToRoleAsync(user, AppRoles.Basic);
                //    }

                //}
            }
        }
    }
}
