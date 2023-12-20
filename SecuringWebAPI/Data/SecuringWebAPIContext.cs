using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecuredWebAPIBestPractices.Model.Domain;
using SecuringWebAPI.Model.Domain;

namespace SecuringWebAPI.Data
{
    public class SecuringWebAPIContext : IdentityDbContext<ApplicationUser>
    {
        public SecuringWebAPIContext(DbContextOptions<SecuringWebAPIContext> options) : base(options)
        {

        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
    }
}
