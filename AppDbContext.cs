using Microsoft.EntityFrameworkCore;
using EshopCrud.Models;

namespace EshopCrud.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }

        public DbSet<Cape> Capes{ get; set; }
        public DbSet<Case> Cases{ get; set; }

        public DbSet<Trendy> Trendy{ get; set; }

        public DbSet<Krytka> Krytky{ get; set; }
       
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nastavenie vzťahov a obmedzení
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);
        }
    }
}



