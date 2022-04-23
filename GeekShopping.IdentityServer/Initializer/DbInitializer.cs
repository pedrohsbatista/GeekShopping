using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Model.Context;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GeekShopping.IdentityServer.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly MySqlContext _mySqlContext;
        private readonly UserManager<ApplicationUser> _user;
        private readonly RoleManager<IdentityRole> _role;

        public DbInitializer(MySqlContext mySqlContext, UserManager<ApplicationUser> user, RoleManager<IdentityRole> role)
        {
            _mySqlContext = mySqlContext;
            _user = user;
            _role = role;
        }

        public void Initialize()
        {
            if (_role.FindByNameAsync(IdentityConfiguration.Admin).Result != null) return;

            _role.CreateAsync(new IdentityRole(IdentityConfiguration.Admin)).GetAwaiter().GetResult();
            _role.CreateAsync(new IdentityRole(IdentityConfiguration.Client)).GetAwaiter().GetResult();

            ApplicationUser admin = new ApplicationUser
            {
                UserName = "Administrador-A",
                Email = "admina@admin.com",
                EmailConfirmed = true,
                PhoneNumber = "+55 (17) 99999-9999",
                FirstName = "Administrador",
                LastName = "A"
            };
          
            _user.CreateAsync(admin, "Abc123.").GetAwaiter().GetResult();
            _user.AddToRoleAsync(admin, IdentityConfiguration.Admin).GetAwaiter().GetResult();          
           
            var adminClaims = _user.AddClaimsAsync(admin, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
                new Claim(JwtClaimTypes.GivenName, $"{admin.FirstName}"),
                new Claim(JwtClaimTypes.FamilyName, $"{admin.LastName}"),
                new Claim(JwtClaimTypes.Role, IdentityConfiguration.Admin)
            }).Result;

            ApplicationUser client = new ApplicationUser
            {
                UserName = "Cliente-A",
                Email = "clientea@cliente.com",
                EmailConfirmed = true,
                PhoneNumber = "+55 (17) 11111-1111",
                FirstName = "Cliente",
                LastName = "A"
            };

            _user.CreateAsync(client, "Abc123.").GetAwaiter().GetResult();
            _user.AddToRoleAsync(client, IdentityConfiguration.Client).GetAwaiter().GetResult();

            var clientClaims = _user.AddClaimsAsync(client, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, $"{client.FirstName} {client.LastName}"),
                new Claim(JwtClaimTypes.GivenName, $"{client.FirstName}"),
                new Claim(JwtClaimTypes.FamilyName, $"{client.LastName}"),
                new Claim(JwtClaimTypes.Role, IdentityConfiguration.Client)
            }).Result;
        }
    }
}
