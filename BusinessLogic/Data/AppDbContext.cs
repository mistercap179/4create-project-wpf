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

        // Configures the model relationships and seed data
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seeds initial data for the Employee table
            modelBuilder.Entity<Employee>().HasData(
                new Employee("John", "Smith", 14) { ID = Guid.NewGuid() },
                new Employee("Jane", "Doe", 11) { ID = Guid.NewGuid() },
                new Employee("Michael", "Jordan", 17) { ID = Guid.NewGuid() }
            );

            // Configures the one-to-many relationship between Employee and Vacation
            modelBuilder.Entity<Vacation>()
                .HasOne<Employee>() // A vacation is associated with one employee
                .WithMany(e => e.Vacations) // An employee can have multiple vacations
                .HasForeignKey(v => v.EmployeeID) // Specifies the foreign key in the Vacation table
                .OnDelete(DeleteBehavior.Cascade); // Ensures vacations are deleted if the related employee is deleted
        }

        // Configures additional settings for the DbContext
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Enables lazy loading of related data
            optionsBuilder.UseLazyLoadingProxies();

            // Suppresses warnings about pending model changes
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
    }
}
