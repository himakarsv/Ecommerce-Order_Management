using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options):base(options) { }


        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Cart> Carts { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductID = 1, Name = "Phone1", Price = 150000 },
                new Product { ProductID = 2, Name = "Phone2", Price = 250000 },
                new Product { ProductID = 3, Name = "Phone3", Price = 350000 },
                new Product { ProductID = 4, Name = "Phone4", Price = 450000 },
                new Product { ProductID = 5, Name = "Phone5", Price = 550000 }
                );

            modelBuilder.Entity<User>()
            .HasDiscriminator<string>("Role")
            .HasValue<Customer>("Customer")
            .HasValue<Admin>("Admin");


            modelBuilder.Entity<Order>()
            .HasOne<Customer>() // or .HasOne(o => o.Customer) if you add nav property
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Customer)
                .WithMany()
                .HasForeignKey(c => c.CustomerId);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId);

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();



        }

    }
}
