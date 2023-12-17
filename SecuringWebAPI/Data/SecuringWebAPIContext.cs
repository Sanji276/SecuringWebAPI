using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecuringWebAPI.Model.Domain;

namespace SecuringWebAPI.Data
{
    public class SecuringWebAPIContext : IdentityDbContext<ApplicationUser>
    {
        public SecuringWebAPIContext(DbContextOptions<SecuringWebAPIContext> options) : base(options)
        {

        }

        public DbSet<TokenInfo> TokenInfos { get; set; }
    }
}
