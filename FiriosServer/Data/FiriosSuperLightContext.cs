using Firios.Entity;
using Microsoft.EntityFrameworkCore;

namespace Firios.Data
{
    public class FiriosSuperLightContext : DbContext
    {
        public FiriosSuperLightContext(DbContextOptions<FiriosSuperLightContext> options)
            : base(options)
        {
        }

        public DbSet<IncidentEntity> IncidentEntity { get; set; }

        public DbSet<UserEntity> UserEntity { get; set; }
        public DbSet<UserBrowserData> UserBrowserDatas { get; set; }
        public DbSet<UserIncidentEntity> UserIncidentEntity { get; set; }
    }
}
