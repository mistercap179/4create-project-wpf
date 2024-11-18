using BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Vacation> Vacations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>().HasData(
                new Employee("John", "Smith", 14) { ID = Guid.NewGuid() },
                new Employee("Jane", "Doe", 11) { ID = Guid.NewGuid() },
                new Employee("Michael", "Jordan", 17) { ID = Guid.NewGuid() }
            );

            // Define the relationship between Employee and Vacation
            modelBuilder.Entity<Vacation>()
                .HasOne<Employee>() // Establishes the relationship with the Employee entity
                .WithMany(e => e.Vacations) // One employee can have multiple vacations
                .HasForeignKey(v => v.EmployeeID) // Sets the foreign key in the Vacation model
                .OnDelete(DeleteBehavior.Cascade); // Deletes vacations if the related employee is deleted
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            //optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
    }
}
