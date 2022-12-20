using FitnessClub.Auth;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.DAL
{
    public class DataContext:DbContext
    {
        public DataContext(DbContextOptions options):base(options) { }

        public DbSet<RegisterModel> registerModels => Set<RegisterModel>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

    }
}
