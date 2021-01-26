using GameContextAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameContextAPI.DataContext

{
    public class GameContext : DbContext
    {
        public GameContext(DbContextOptions<GameContext> options) : base(options)
        {


        }
// Note: Database clustered index settings done by SQL statements, migrations will not add optimized storing settings, for table optimization user table should be indexed from SQL Server
        public DbSet<User> Users { get; set; }

    }
}
