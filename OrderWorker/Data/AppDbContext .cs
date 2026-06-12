using Microsoft.EntityFrameworkCore;

namespace OrderWorker.Data
{
    public class AppDbContext : DbContext
    {                
        public DbSet<Order> Orders { get; set; }
        public DbSet<CarSale> CarSales { get; set; }

        public AppDbContext (DbContextOptions<AppDbContext> options):base(options)
        {
            
        }
    }
}